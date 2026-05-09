using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Cinema.Shared.Models;
using Booking.API.Infrastructure.Hubs;
using Booking.API.Infrastructure.Hubs.Interfaces;
using Booking.API.Infrastructure.Hubs.Services;
using Microsoft.AspNetCore.SignalR;

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
    private readonly IHubContext<BookingHub, IBookingHubClient> _bookingHubContext;
    private readonly ILogger<PaymentCompletedIntegrationEventHandler> _logger;

    public PaymentCompletedIntegrationEventHandler(
        IBookingService bookingService,
        IEmailService emailService,
        IAdminDashboardNotificationService adminDashboardNotificationService,
        IHubContext<BookingHub, IBookingHubClient> bookingHubContext,
        ILogger<PaymentCompletedIntegrationEventHandler> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _adminDashboardNotificationService = adminDashboardNotificationService ?? throw new ArgumentNullException(nameof(adminDashboardNotificationService));
        _bookingHubContext = bookingHubContext ?? throw new ArgumentNullException(nameof(bookingHubContext));
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

                // Step 2: Push SignalR notification IMMEDIATELY (don't wait for email)
                try
                {
                    var groupName = $"booking-{@event.BookingId}";
                    await _bookingHubContext.Clients
                        .Group(groupName)
                        .BookingConfirmed(@event.BookingId, "Confirmed");

                    _logger.LogInformation(
                        "SignalR notification sent to group {GroupName} for booking {BookingId}",
                        groupName, @event.BookingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send SignalR notification for booking {BookingId}",
                        @event.BookingId);
                }

                // Step 3: Fetch booking details and send email ASYNC (don't block)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var bookingResult = await _bookingService.GetBookingByIdAsync(@event.BookingId);

                        if (bookingResult.Success && bookingResult.Data != null)
                        {
                            var booking = bookingResult.Data;

                            // Send confirmation email to customer
                            var emailDto = new BookingEmailDto
                            {
                                Id = booking.BookingId,
                                BookingCode = booking.BookingId.ToString().Substring(0, 8).ToUpper(),
                                CustomerEmail = @event.CustomerEmail,
                                CustomerName = @event.CustomerName,
                                MovieTitle = booking.ShowtimeDetails?.MovieTitle ?? "Unknown Movie",
                                CinemaName = booking.ShowtimeDetails?.CinemaName ?? "Unknown Cinema",
                                CinemaHallName = booking.ShowtimeDetails?.CinemaHallName ?? "Unknown Hall",
                                ShowtimeDate = booking.ShowtimeDetails?.StartTime ?? DateTime.UtcNow,
                                TotalAmount = booking.TotalPrice,
                                Status = booking.Status,
                                BookingSeats = booking.Seats.Select(s => new Application.DTOs.BookingSeatDto
                                {
                                    SeatNumber = $"{s.Row}{s.Number}",
                                    SeatPrice = s.Price
                                }).ToList()
                            };

                            var emailSent = await _emailService.SendBookingConfirmationAsync(emailDto);

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

                            await _adminDashboardNotificationService.PublishBookingCompletedAsync(
                                @event.BookingId,
                                @event.Amount,
                                @event.CustomerName,
                                @event.CompletedAt);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Could not fetch booking details for email. Booking {BookingId}",
                                @event.BookingId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error sending email for booking {BookingId}",
                            @event.BookingId);
                    }
                });
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
}
