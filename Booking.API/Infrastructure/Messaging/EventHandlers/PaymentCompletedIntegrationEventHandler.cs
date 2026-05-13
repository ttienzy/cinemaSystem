using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Cinema.Shared.Models;
using Booking.API.Infrastructure.Hubs.Services;

namespace Booking.API.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handles PaymentCompletedIntegrationEvent from Payment service
/// </summary>
public class PaymentCompletedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentCompletedIntegrationEvent>
{
    private readonly IBookingService _bookingService;
    private readonly IEmailService _emailService;
    private readonly IAdminDashboardNotificationService _adminDashboardNotificationService;
    private readonly ILogger<PaymentCompletedIntegrationEventHandler> _logger;

    public PaymentCompletedIntegrationEventHandler(
        IBookingService bookingService,
        IEmailService emailService,
        IAdminDashboardNotificationService adminDashboardNotificationService,
        ILogger<PaymentCompletedIntegrationEventHandler> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _adminDashboardNotificationService = adminDashboardNotificationService ?? throw new ArgumentNullException(nameof(adminDashboardNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PaymentCompletedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "Handling PaymentCompletedIntegrationEvent for booking {BookingId}, transaction {TransactionId}",
            @event.BookingId,
            @event.TransactionId);

        try
        {
            // Step 1: Confirm the booking (change status from Pending to Confirmed)
            var result = await _bookingService.ConfirmBookingAsync(
                @event.BookingId,
                @event.TransactionId);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Successfully confirmed booking {BookingId} after payment completion",
                    @event.BookingId);

                await SendConfirmationEmailAsync(@event);
                await PublishDashboardActivityAsync(@event);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to confirm booking {BookingId}: {Message}",
                    @event.BookingId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentCompletedIntegrationEvent for booking {BookingId}",
                @event.BookingId);

            // Re-throw to trigger retry mechanism (if configured)
            throw;
        }
    }

    private async Task SendConfirmationEmailAsync(PaymentCompletedIntegrationEvent @event)
    {
        try
        {
            var bookingResult = await _bookingService.GetBookingByIdAsync(@event.BookingId);
            if (!bookingResult.Success || bookingResult.Data is null)
            {
                _logger.LogWarning(
                    "Could not fetch booking details for email. Booking {BookingId}",
                    @event.BookingId);
                return;
            }

            var emailSent = await _emailService.SendBookingConfirmationAsync(
                BuildBookingConfirmationEmail(bookingResult.Data, @event));

            if (emailSent)
            {
                _logger.LogInformation(
                    "Confirmation email sent successfully for booking {BookingId} to {Email}",
                    @event.BookingId,
                    @event.CustomerEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send confirmation email for booking {BookingId} to {Email}",
                    @event.BookingId,
                    @event.CustomerEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending email for booking {BookingId}",
                @event.BookingId);
        }
    }

    private async Task PublishDashboardActivityAsync(PaymentCompletedIntegrationEvent @event)
    {
        try
        {
            await _adminDashboardNotificationService.PublishBookingCompletedAsync(
                @event.BookingId,
                @event.Amount,
                @event.CustomerName,
                @event.CompletedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing dashboard activity for booking {BookingId}",
                @event.BookingId);
        }
    }

    private static BookingEmailDto BuildBookingConfirmationEmail(
        BookingResponse booking,
        PaymentCompletedIntegrationEvent @event)
    {
        return new BookingEmailDto
        {
            Id = booking.BookingId,
            BookingCode = booking.BookingId.ToString("N")[..8].ToUpperInvariant(),
            CustomerEmail = @event.CustomerEmail,
            CustomerName = @event.CustomerName,
            MovieTitle = booking.ShowtimeDetails?.MovieTitle ?? "Unknown Movie",
            CinemaName = booking.ShowtimeDetails?.CinemaName ?? "Unknown Cinema",
            CinemaHallName = booking.ShowtimeDetails?.CinemaHallName ?? "Unknown Hall",
            ShowtimeDate = booking.ShowtimeDetails?.StartTime ?? DateTime.UtcNow,
            TotalAmount = @event.Amount,
            Status = booking.Status,
            BookingSeats = booking.Seats.Select(seat => new Application.DTOs.BookingSeatDto
            {
                SeatNumber = $"{seat.Row}{seat.Number}",
                SeatPrice = seat.Price
            }).ToList()
        };
    }
}
