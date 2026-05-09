using Booking.API.Application.DTOs.External;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Domain.Entities;
using Booking.API.Infrastructure.Integrations.Clients;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;
using BookingSeatResponseDto = Booking.API.Application.DTOs.Responses.BookingSeatDto;

namespace Booking.API.Application.Services;

public class TicketOperationsService : ITicketOperationsService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IExternalServiceClient _externalClient;
    private readonly PaymentApiClient _paymentApiClient;
    private readonly ILogger<TicketOperationsService> _logger;

    public TicketOperationsService(
        IBookingRepository bookingRepository,
        IExternalServiceClient externalClient,
        PaymentApiClient paymentApiClient,
        ILogger<TicketOperationsService> logger)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
        _paymentApiClient = paymentApiClient ?? throw new ArgumentNullException(nameof(paymentApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(
        string? query,
        int pageNumber,
        int pageSize)
    {
        var paymentPage = await _paymentApiClient.SearchPaymentsAsync(query, pageNumber, pageSize);
        var bookingIds = paymentPage.Items.Select(x => x.BookingId).Distinct().ToList();

        if (bookingIds.Count == 0)
        {
            return ApiResponse<PaginatedResponse<TicketOperationResponse>>.SuccessResponse(
                PaginatedResponse<TicketOperationResponse>.Create([], 0, paymentPage.PageNumber, paymentPage.PageSize),
                "No tickets found");
        }

        var bookings = await _bookingRepository.GetByIdsWithSeatsAsync(bookingIds);
        var bookingMap = bookings.ToDictionary(x => x.Id);

        var items = new List<TicketOperationResponse>();
        foreach (var payment in paymentPage.Items)
        {
            if (!bookingMap.TryGetValue(payment.BookingId, out var booking))
            {
                _logger.LogWarning("Payment {PaymentId} references missing booking {BookingId}", payment.PaymentId, payment.BookingId);
                continue;
            }

            items.Add(await MapToTicketOperationResponseAsync(booking, payment));
        }

        var response = PaginatedResponse<TicketOperationResponse>.Create(
            items,
            paymentPage.TotalCount,
            paymentPage.PageNumber,
            paymentPage.PageSize);

        return ApiResponse<PaginatedResponse<TicketOperationResponse>>.SuccessResponse(
            response,
            $"Found {response.TotalCount} ticket(s)");
    }

    public async Task<ApiResponse<TicketOperationResponse>> CheckInAsync(Guid bookingId, string staffUserId)
    {
        var booking = await _bookingRepository.GetByIdWithSeatsAsync(bookingId);
        if (booking == null)
        {
            return ApiResponse<TicketOperationResponse>.NotFoundResponse($"Booking {bookingId} not found");
        }

        var payment = await _paymentApiClient.GetPaymentByBookingIdAsync(bookingId);
        if (payment == null)
        {
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                "Payment information not found for this booking",
                400);
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                "Ticket has already been checked in",
                400,
                [new ErrorDetail("ALREADY_CHECKED_IN", "Ticket has already been checked in")]);
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                "Only paid bookings can be checked in",
                400,
                [new ErrorDetail("INVALID_BOOKING_STATUS", $"Current booking status is {booking.Status}")]);
        }

        if (payment.Status != PaymentLookupStatus.Completed)
        {
            return ApiResponse<TicketOperationResponse>.FailureResponse(
                "Only paid tickets can be checked in",
                400,
                [new ErrorDetail("PAYMENT_NOT_COMPLETED", $"Current payment status is {payment.Status}")]);
        }

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        _logger.LogInformation("Booking {BookingId} checked in by staff {StaffUserId}", bookingId, staffUserId);

        var response = await MapToTicketOperationResponseAsync(booking, payment);
        return ApiResponse<TicketOperationResponse>.SuccessResponse(response, "Ticket checked in successfully");
    }

    private async Task<TicketOperationResponse> MapToTicketOperationResponseAsync(
        Booking.API.Domain.Entities.Booking booking,
        PaymentLookupDto payment)
    {
        var showtime = await _externalClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        ShowtimeDetailsDto? showtimeDetails = null;
        var seats = new List<BookingSeatResponseDto>();

        if (showtime != null)
        {
            var movie = await _externalClient.GetMovieByIdAsync(showtime.MovieId);
            var cinemaHall = await _externalClient.GetCinemaHallByIdAsync(showtime.CinemaHallId);
            var hallSeats = await _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);

            showtimeDetails = new ShowtimeDetailsDto
            {
                ShowtimeId = showtime.Id,
                MovieTitle = movie?.Title ?? "Unknown",
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                CinemaName = "Cinema",
                CinemaHallName = cinemaHall?.Name ?? "Unknown"
            };

            seats = booking.BookingSeats
                .Select(bookingSeat =>
                {
                    var seat = hallSeats.FirstOrDefault(s => s.Id == bookingSeat.SeatId);
                    return seat == null
                        ? null
                        : new BookingSeatResponseDto
                        {
                            SeatId = seat.Id,
                            Row = seat.Row,
                            Number = seat.Number,
                            Price = bookingSeat.Price
                        };
                })
                .Where(x => x != null)
                .Cast<BookingSeatResponseDto>()
                .ToList();
        }

        return new TicketOperationResponse
        {
            BookingId = booking.Id,
            TicketCode = payment.OrderInvoiceNumber,
            CustomerName = payment.CustomerName,
            CustomerEmail = payment.CustomerEmail,
            CustomerPhone = payment.CustomerPhone,
            BookingStatus = booking.Status,
            PaymentStatus = payment.Status,
            OperationalStatus = MapOperationalStatus(booking.Status, payment.Status),
            CanCheckIn = booking.Status == BookingStatus.Confirmed && payment.Status == PaymentLookupStatus.Completed,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            PaidAt = payment.CompletedAt,
            CheckedInAt = booking.Status == BookingStatus.CheckedIn ? booking.UpdatedAt : null,
            Seats = seats,
            ShowtimeDetails = showtimeDetails
        };
    }

    private static string MapOperationalStatus(BookingStatus bookingStatus, PaymentLookupStatus paymentStatus)
    {
        return bookingStatus switch
        {
            BookingStatus.CheckedIn => "CheckedIn",
            BookingStatus.Cancelled => paymentStatus == PaymentLookupStatus.Refunded ? "Refunded" : "Cancelled",
            BookingStatus.Expired => "Expired",
            BookingStatus.Confirmed when paymentStatus == PaymentLookupStatus.Completed => "Paid",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Processing => "ProcessingPayment",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Failed => "PaymentFailed",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Cancelled => "PaymentCancelled",
            _ => bookingStatus.ToString()
        };
    }
}
