using Booking.API.Infrastructure.Integrations.Clients;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

public class TicketOperationsService : ITicketOperationsService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly PaymentApiClient _paymentApiClient;
    private readonly ITicketOperationResponseFactory _ticketOperationResponseFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketOperationsService> _logger;

    public TicketOperationsService(
        IBookingRepository bookingRepository,
        PaymentApiClient paymentApiClient,
        ITicketOperationResponseFactory ticketOperationResponseFactory,
        IUnitOfWork unitOfWork,
        ILogger<TicketOperationsService> logger)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _paymentApiClient = paymentApiClient ?? throw new ArgumentNullException(nameof(paymentApiClient));
        _ticketOperationResponseFactory = ticketOperationResponseFactory ?? throw new ArgumentNullException(nameof(ticketOperationResponseFactory));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(
        string? query,
        int pageNumber,
        int pageSize)
    {
        var paymentPage = await _paymentApiClient.SearchPaymentsAsync(query, pageNumber, pageSize);
        var bookingIds = paymentPage.Items
            .Select(payment => payment.BookingId)
            .Distinct()
            .ToList();

        if (bookingIds.Count == 0)
        {
            return ApiResponse<PaginatedResponse<TicketOperationResponse>>.SuccessResponse(
                PaginatedResponse<TicketOperationResponse>.Create([], 0, paymentPage.PageNumber, paymentPage.PageSize),
                TicketOperationException.NO_TICKETS_FOUND);
        }

        var bookings = await _bookingRepository.GetByIdsWithSeatsAsync(bookingIds);
        var bookingMap = bookings.ToDictionary(booking => booking.Id);
        var items = await BuildTicketResponsesAsync(paymentPage.Items, bookingMap);

        var response = PaginatedResponse<TicketOperationResponse>.Create(
            items,
            paymentPage.TotalCount,
            paymentPage.PageNumber,
            paymentPage.PageSize);

        return ApiResponse<PaginatedResponse<TicketOperationResponse>>.SuccessResponse(
            response,
            $"Found {response.TotalCount} {TicketOperationException.TICKETS_FOUND}");
    }

    public async Task<ApiResponse<TicketOperationResponse>> CheckInAsync(Guid bookingId, string staffUserId)
    {
        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse<TicketOperationResponse>.NotFoundResponse($"Booking {bookingId} not found");
        }

        var payment = await _paymentApiClient.GetPaymentByBookingIdAsync(bookingId);
        var validationResult = ValidateCheckInEligibility(booking, payment);
        if (validationResult != null)
        {
            return validationResult;
        }

        await ExecuteInTransactionAsync(nameof(CheckInAsync), async () =>
        {
            booking.MarkCheckedIn(DateTime.UtcNow);
            await _bookingRepository.UpdateAsync(booking);
        });

        _logger.LogInformation("Booking {BookingId} checked in by staff {StaffUserId}", bookingId, staffUserId);

        var response = await _ticketOperationResponseFactory.CreateAsync(booking, payment!);
        return ApiResponse<TicketOperationResponse>.SuccessResponse(
            response,
            TicketOperationException.TICKET_CHECKED_IN_SUCCESSFULLY);
    }

    private async Task<List<TicketOperationResponse>> BuildTicketResponsesAsync(
        IReadOnlyCollection<PaymentLookupDto> payments,
        IReadOnlyDictionary<Guid, Booking.API.Domain.Entities.Booking> bookingMap)
    {
        var items = new List<TicketOperationResponse>();

        foreach (var payment in payments)
        {
            if (!bookingMap.TryGetValue(payment.BookingId, out var booking))
            {
                _logger.LogWarning(
                    "Payment {PaymentId} references missing booking {BookingId}",
                    payment.PaymentId,
                    payment.BookingId);
                continue;
            }

            items.Add(await _ticketOperationResponseFactory.CreateAsync(booking, payment));
        }

        return items;
    }

    private static ApiResponse<TicketOperationResponse>? ValidateCheckInEligibility(
        Booking.API.Domain.Entities.Booking booking,
        PaymentLookupDto? payment)
    {
        if (payment == null)
        {
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                TicketOperationException.PAYMENT_NOT_FOUND_FOR_BOOKING,
                400);
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            var value = TicketOperationException.ALREADY_CHECKED_IN;
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                TicketOperationException.ALREADY_CHECKED_IN_MESSAGE,
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            var value = TicketOperationException.INVALID_BOOKING_STATUS(booking.Status);
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                TicketOperationException.INVALID_BOOKING_STATUS_MESSAGE,
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        if (payment.Status != PaymentLookupStatus.Completed)
        {
            var value = TicketOperationException.PAYMENT_NOT_COMPLETED(payment.Status);
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                TicketOperationException.PAYMENT_NOT_COMPLETED_MESSAGE,
                400,
                [new ErrorDetail(value.Code, value.Message, value.Field)]);
        }

        return null;
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
                "Rollback failed for ticket operation {OperationName}",
                operationName);
        }
    }
}
