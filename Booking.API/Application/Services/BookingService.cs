using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Contracts.Events;
using Cinema.Shared.Models;
using MassTransit;
using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

/// <summary>
/// Orchestration service for booking operations
/// Coordinates domain state transitions, persistence, external seat state and integration events.
/// </summary>
public class BookingService : IBookingService
{
    private const int DefaultBookingExpirationMinutes = 10;

    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingCreationPreparationService _bookingCreationPreparationService;
    private readonly IBookingResponseFactory _bookingResponseFactory;
    private readonly ISeatStatusService _seatStatusService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingService> _logger;
    private readonly IConfiguration _configuration;

    public BookingService(
        IBookingRepository bookingRepository,
        IBookingCreationPreparationService bookingCreationPreparationService,
        IBookingResponseFactory bookingResponseFactory,
        ISeatStatusService seatStatusService,
        IPublishEndpoint publishEndpoint,
        IUnitOfWork unitOfWork,
        ILogger<BookingService> logger,
        IConfiguration configuration)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _bookingCreationPreparationService = bookingCreationPreparationService ?? throw new ArgumentNullException(nameof(bookingCreationPreparationService));
        _bookingResponseFactory = bookingResponseFactory ?? throw new ArgumentNullException(nameof(bookingResponseFactory));
        _seatStatusService = seatStatusService ?? throw new ArgumentNullException(nameof(seatStatusService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
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

        await TryPublishBookingCreatedEventAsync(booking, request);
        var response = await _bookingResponseFactory.CreateAsync(booking);

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
        await TryPublishBookingCancelledEventAsync(
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
        await TryPublishBookingExpiredEventAsync(booking, seatIds);

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

    private async Task TryPublishBookingCreatedEventAsync(
        BookingEntity booking,
        CreateBookingRequest request)
    {
        try
        {
            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                ShowtimeId = booking.ShowtimeId,
                SeatIds = booking.GetSeatIds(),
                TotalPrice = booking.TotalPrice,
                BookingDate = booking.BookingDate,
                CustomerEmail = request.ContactEmail,
                CustomerPhone = request.ContactPhone,
                CustomerName = request.ContactName,
                OccurredAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Published BookingCreatedEvent for booking {BookingId}. Client should poll payment status.",
                booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingCreatedEvent for booking {BookingId}", booking.Id);
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

    private async Task TryPublishBookingCancelledEventAsync(
        BookingEntity booking,
        List<Guid> seatIds,
        bool needsRefund,
        string reason)
    {
        try
        {
            await _publishEndpoint.Publish(new BookingCancelledEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                ShowtimeId = booking.ShowtimeId,
                SeatIds = seatIds,
                NeedsRefund = needsRefund,
                Reason = reason,
                OccurredAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingCancelledEvent for booking {BookingId}", booking.Id);
        }
    }

    private async Task TryPublishBookingExpiredEventAsync(BookingEntity booking, List<Guid> seatIds)
    {
        try
        {
            await _publishEndpoint.Publish(new BookingExpiredEvent
            {
                BookingId = booking.Id,
                ShowtimeId = booking.ShowtimeId,
                SeatIds = seatIds,
                ExpiredAt = DateTime.UtcNow,
                OccurredAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingExpiredEvent for booking {BookingId}", booking.Id);
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

}

