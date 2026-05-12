using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Cinema.Shared.Models;
using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

/// <summary>
/// Orchestration service for booking operations
/// Coordinates domain state transitions, persistence, external seat state and integration events.
/// </summary>
public class BookingService : IBookingService
{
    private const int DefaultBookingExpirationMinutes = 10;
    private const int DefaultPaymentCreationMaxRetries = 8;
    private const int DefaultPaymentCreationInitialDelayMs = 200;

    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingCreationPreparationService _bookingCreationPreparationService;
    private readonly IBookingResponseFactory _bookingResponseFactory;
    private readonly ISeatStatusService _seatStatusService;
    private readonly PaymentApiClient _paymentApiClient;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingService> _logger;
    private readonly IConfiguration _configuration;

    public BookingService(
        IBookingRepository bookingRepository,
        IBookingCreationPreparationService bookingCreationPreparationService,
        IBookingResponseFactory bookingResponseFactory,
        ISeatStatusService seatStatusService,
        PaymentApiClient paymentApiClient,
        IEventBus eventBus,
        IUnitOfWork unitOfWork,
        ILogger<BookingService> logger,
        IConfiguration configuration)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _bookingCreationPreparationService = bookingCreationPreparationService ?? throw new ArgumentNullException(nameof(bookingCreationPreparationService));
        _bookingResponseFactory = bookingResponseFactory ?? throw new ArgumentNullException(nameof(bookingResponseFactory));
        _seatStatusService = seatStatusService ?? throw new ArgumentNullException(nameof(seatStatusService));
        _paymentApiClient = paymentApiClient ?? throw new ArgumentNullException(nameof(paymentApiClient));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request)
    {
        _logger.LogInformation(
            "Creating booking for user {UserId}, showtime {ShowtimeId}, seats: {SeatCount}",
            request.UserId,
            request.ShowtimeId,
            request.SeatIds.Count);

        var preparation = await _bookingCreationPreparationService.PrepareAsync(request);
        if (!preparation.Success)
        {
            return preparation.FailureResponse!;
        }

        var bookingId = Guid.NewGuid();
        var seatBookingResult = await _seatStatusService.VerifyAndMarkAsBookedAsync(
            request.ShowtimeId,
            request.SeatIds,
            request.UserId,
            bookingId);

        if (!seatBookingResult.Success)
        {
            return BuildSeatBookingFailureResponse(seatBookingResult);
        }

        var nowUtc = DateTime.UtcNow;
        var booking = BookingEntity.CreatePending(
            bookingId,
            request.UserId,
            request.ShowtimeId,
            preparation.SelectedSeats.Select(seat => seat.Id),
            preparation.Showtime!.Price,
            nowUtc,
            nowUtc.AddMinutes(GetBookingExpirationMinutes()));

        try
        {
            await ExecuteInTransactionAsync(nameof(CreateBookingAsync), async () =>
            {
                await _bookingRepository.CreateAsync(booking);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking record {BookingId}, releasing seats", booking.Id);
            await TryReleaseSeatsAsync(request.ShowtimeId, request.SeatIds, nameof(CreateBookingAsync));

            var value = BookingException.DATABASE_ERROR(ex.Message);
            return ApiResponse<BookingResponse>.FailureResponse(
                BookingException.BOOKING_CREATION_FAILED,
                500,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        _logger.LogInformation("Booking {BookingId} created successfully", booking.Id);

        var paymentCheckout = await TryCreatePaymentCheckoutAsync(booking, request);
        var response = await _bookingResponseFactory.CreateAsync(booking, paymentCheckout);

        return ApiResponse<BookingResponse>.SuccessResponse(
            response,
            BookingException.BOOKING_CREATED_SUCCESSFULLY,
            201);
    }

    public async Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid bookingId)
    {
        _logger.LogInformation("Getting booking {BookingId}", bookingId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse<BookingResponse>.NotFoundResponse(
                BookingException.BOOKING_NOT_FOUND(bookingId));
        }

        var response = await _bookingResponseFactory.CreateAsync(booking);
        return ApiResponse<BookingResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<List<BookingResponse>>> GetUserBookingsAsync(string userId)
    {
        _logger.LogInformation("Getting bookings for user {UserId}", userId);

        var bookings = await _bookingRepository.GetByUserIdAsync(userId);
        var responses = new List<BookingResponse>();

        foreach (var booking in bookings)
        {
            responses.Add(await _bookingResponseFactory.CreateAsync(booking));
        }

        return ApiResponse<List<BookingResponse>>.SuccessResponse(
            responses,
            BookingException.USER_BOOKINGS_FOUND(responses.Count));
    }

    public async Task<ApiResponse> CancelBookingAsync(Guid bookingId, CancelBookingRequest request)
    {
        _logger.LogInformation(
            "Cancelling booking {BookingId} by user {UserId}",
            bookingId,
            request.UserId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse(BookingException.BOOKING_NOT_FOUND(bookingId));
        }

        if (!booking.IsOwnedBy(request.UserId))
        {
            return ApiResponse.UnauthorizedResponse(BookingException.UNAUTHORIZED_CANCELLATION);
        }

        var validationResponse = ValidateCancellation(booking);
        if (validationResponse != null)
        {
            return validationResponse;
        }

        var seatIds = booking.GetSeatIds();
        var needsRefund = booking.NeedsRefundOnCancellation();

        try
        {
            await ExecuteInTransactionAsync(nameof(CancelBookingAsync), async () =>
            {
                booking.MarkCancelled(DateTime.UtcNow);
                await _bookingRepository.UpdateAsync(booking);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return ApiResponse.InternalServerErrorResponse(BookingException.BOOKING_CANCELLATION_FAILED);
        }

        await TryReleaseSeatsAsync(booking.ShowtimeId, seatIds, nameof(CancelBookingAsync));
        TryPublishBookingCancelledEvent(
            booking,
            seatIds,
            needsRefund,
            request.CancellationReason ?? BookingException.USER_CANCELLED_REASON);

        _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);
        return ApiResponse.SuccessResponse(BookingException.BOOKING_CANCELLED_SUCCESSFULLY);
    }

    public async Task<ApiResponse> ConfirmBookingAsync(Guid bookingId, string transactionId)
    {
        _logger.LogInformation(
            "Confirming booking {BookingId} with transaction {TransactionId}",
            bookingId,
            transactionId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse(BookingException.BOOKING_NOT_FOUND(bookingId));
        }

        if (!booking.IsPending())
        {
            var value = BookingException.INVALID_CONFIRM_STATUS(booking.Status);
            return ApiResponse.FailureResponse(
                BookingException.INVALID_CONFIRM_STATUS_MESSAGE(booking.Status),
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        try
        {
            await ExecuteInTransactionAsync(nameof(ConfirmBookingAsync), async () =>
            {
                booking.MarkConfirmed(DateTime.UtcNow);
                await _bookingRepository.UpdateAsync(booking);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            return ApiResponse.InternalServerErrorResponse(BookingException.BOOKING_CONFIRMATION_FAILED);
        }

        _logger.LogInformation("Booking {BookingId} confirmed successfully", bookingId);
        return ApiResponse.SuccessResponse(BookingException.BOOKING_CONFIRMED_SUCCESSFULLY);
    }

    public async Task<ApiResponse> ExpireBookingAsync(Guid bookingId)
    {
        _logger.LogInformation("Expiring booking {BookingId}", bookingId);

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse.NotFoundResponse(BookingException.BOOKING_NOT_FOUND(bookingId));
        }

        if (!booking.IsPending())
        {
            _logger.LogWarning("Cannot expire booking {BookingId} - status is {Status}", bookingId, booking.Status);

            var value = BookingException.ONLY_PENDING_CAN_EXPIRE;
            return ApiResponse.FailureResponse(
                BookingException.INVALID_EXPIRE_STATUS_MESSAGE(booking.Status),
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        var seatIds = booking.GetSeatIds();

        try
        {
            await ExecuteInTransactionAsync(nameof(ExpireBookingAsync), async () =>
            {
                booking.MarkExpired(DateTime.UtcNow);
                await _bookingRepository.UpdateAsync(booking);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring booking {BookingId}", bookingId);
            return ApiResponse.InternalServerErrorResponse(BookingException.BOOKING_EXPIRATION_FAILED);
        }

        await TryReleaseSeatsAsync(booking.ShowtimeId, seatIds, nameof(ExpireBookingAsync));
        TryPublishBookingExpiredEvent(booking, seatIds);

        _logger.LogInformation("Booking {BookingId} expired successfully", bookingId);
        return ApiResponse.SuccessResponse(BookingException.BOOKING_EXPIRED_SUCCESSFULLY);
    }

    private ApiResponse<BookingResponse> BuildSeatBookingFailureResponse(SeatBookingResult bookingResult)
    {
        var errorCode = bookingResult.FailureReason switch
        {
            SeatBookingFailureReason.NotLocked => "SEATS_NOT_LOCKED",
            SeatBookingFailureReason.LockExpired => "LOCK_EXPIRED",
            SeatBookingFailureReason.WrongUser => "SEATS_LOCKED_BY_OTHER_USER",
            SeatBookingFailureReason.AlreadyBooked => "SEATS_ALREADY_BOOKED",
            _ => "SEATS_UNAVAILABLE"
        };

        var statusCode = bookingResult.FailureReason == SeatBookingFailureReason.LockExpired ? 410 : 409;
        var message = bookingResult.Message ?? "Cannot book seats";

        return ApiResponse<BookingResponse>.FailureResponse(
            message,
            statusCode,
            [new ErrorDetail(errorCode, bookingResult.Message ?? "Seats are not available for booking", "SeatIds")]);
    }

    private static ApiResponse? ValidateCancellation(BookingEntity booking)
    {
        if (booking.IsCancelled())
        {
            var value = BookingException.ALREADY_CANCELLED;
            return ApiResponse.FailureResponse(
                BookingException.ALREADY_CANCELLED_MESSAGE,
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        if (booking.IsExpired())
        {
            var value = BookingException.BOOKING_EXPIRED;
            return ApiResponse.FailureResponse(
                BookingException.BOOKING_EXPIRED_MESSAGE,
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        return null;
    }

    private async Task<PaymentCheckoutDto?> TryCreatePaymentCheckoutAsync(
        BookingEntity booking,
        CreateBookingRequest request)
    {
        try
        {
            _eventBus.Publish(new BookingCreatedIntegrationEvent(
                booking.Id,
                booking.UserId,
                booking.ShowtimeId,
                booking.GetSeatIds(),
                booking.TotalPrice,
                booking.BookingDate,
                request.ContactEmail,
                request.ContactPhone,
                request.ContactName));

            _logger.LogInformation("Waiting for payment creation for booking {BookingId}", booking.Id);

            var paymentCheckout = await _paymentApiClient.WaitForPaymentCreationAsync(
                booking.Id,
                maxRetries: GetPaymentCreationMaxRetries(),
                initialDelayMs: GetPaymentCreationInitialDelayMs());

            if (paymentCheckout != null)
            {
                _logger.LogInformation(
                    "Booking {BookingId} created with payment {PaymentId}, checkout URL: {CheckoutUrl}",
                    booking.Id,
                    paymentCheckout.PaymentId,
                    paymentCheckout.CheckoutUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Booking {BookingId} created but payment was not ready in time. Client should poll for payment.",
                    booking.Id);
            }

            return paymentCheckout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment orchestration failed for booking {BookingId}", booking.Id);
            return null;
        }
    }

    private async Task TryReleaseSeatsAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string operationName)
    {
        try
        {
            var released = await _seatStatusService.ReleaseBookedSeatsAsync(showtimeId, seatIds);
            if (!released)
            {
                _logger.LogWarning(
                    "Seat release returned false for booking operation {OperationName}, showtime {ShowtimeId}",
                    operationName,
                    showtimeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Seat release failed for booking operation {OperationName}, showtime {ShowtimeId}",
                operationName,
                showtimeId);
        }
    }

    private void TryPublishBookingCancelledEvent(
        BookingEntity booking,
        List<Guid> seatIds,
        bool needsRefund,
        string reason)
    {
        try
        {
            _eventBus.Publish(new BookingCancelledIntegrationEvent(
                booking.Id,
                booking.UserId,
                booking.ShowtimeId,
                seatIds,
                needsRefund,
                reason));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingCancelledIntegrationEvent for booking {BookingId}", booking.Id);
        }
    }

    private void TryPublishBookingExpiredEvent(BookingEntity booking, List<Guid> seatIds)
    {
        try
        {
            _eventBus.Publish(new BookingExpiredIntegrationEvent(
                booking.Id,
                booking.ShowtimeId,
                seatIds,
                DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingExpiredIntegrationEvent for booking {BookingId}", booking.Id);
        }
    }

    private async Task ExecuteInTransactionAsync(string operationName, Func<Task> action)
    {
        var transactionStarted = false;

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            transactionStarted = true;

            await action();
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transactionStarted)
            {
                await TryRollbackAsync(operationName);
            }

            throw;
        }
    }

    private async Task TryRollbackAsync(string operationName)
    {
        try
        {
            await _unitOfWork.RollbackAsync();
        }
        catch (Exception rollbackException)
        {
            _logger.LogError(
                rollbackException,
                "Rollback failed for booking operation {OperationName}",
                operationName);
        }
    }

    private int GetBookingExpirationMinutes()
    {
        return _configuration.GetValue<int>("Booking:ExpirationMinutes", DefaultBookingExpirationMinutes);
    }

    private int GetPaymentCreationMaxRetries()
    {
        return _configuration.GetValue<int>("Booking:PaymentCreationMaxRetries", DefaultPaymentCreationMaxRetries);
    }

    private int GetPaymentCreationInitialDelayMs()
    {
        return _configuration.GetValue<int>("Booking:PaymentCreationInitialDelayMs", DefaultPaymentCreationInitialDelayMs);
    }
}
