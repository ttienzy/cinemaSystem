
using Booking.API.Hubs.Services;
using Booking.API.Notifications.Email;
using Booking.API.Services;
using Cinema.Contracts.Events;
using MassTransit;

namespace Booking.API.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IBookingService _bookingService;
    private readonly IBookingConfirmationEmailService _bookingConfirmationEmailService;
    private readonly IBookingNotificationService _bookingNotificationService;
    private readonly IAdminDashboardNotificationService _adminDashboardNotificationService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IBookingService bookingService,
        IBookingConfirmationEmailService bookingConfirmationEmailService,
        IBookingNotificationService bookingNotificationService,
        IAdminDashboardNotificationService adminDashboardNotificationService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _bookingService = bookingService;
        _bookingConfirmationEmailService = bookingConfirmationEmailService;
        _bookingNotificationService = bookingNotificationService;
        _adminDashboardNotificationService = adminDashboardNotificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed PaymentCompletedEvent for booking {BookingId}, transaction {TransactionId}",
            message.BookingId,
            message.TransactionId);

        try
        {
            // Step 1: Confirm the booking (change status from Pending to Confirmed)
            var result = await _bookingService.ConfirmBookingAsync(
                message.BookingId,
                message.TransactionId);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Successfully confirmed booking {BookingId} after payment completion",
                    message.BookingId);

                await NotifyBookingConfirmedAsync(message, context.CancellationToken);
                await SendConfirmationEmailAsync(message, context.CancellationToken);
                await PublishDashboardActivityAsync(message, context.CancellationToken);
            }
            else
            {
                if (result.StatusCode >= 500 || result.StatusCode == 404)
                {
                    throw new InvalidOperationException(
                        $"Failed to confirm booking {message.BookingId}: {result.Message}");
                }

                _logger.LogWarning(
                    "Skipping PaymentCompletedEvent for booking {BookingId}: {Message}",
                    message.BookingId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentCompletedEvent for booking {BookingId}",
                message.BookingId);

            throw; // Re-throw to trigger MassTransit retry
        }
    }

    private async Task NotifyBookingConfirmedAsync(
        PaymentCompletedEvent message,
        CancellationToken cancellationToken)
    {
        await _bookingNotificationService.NotifyBookingConfirmedAsync(
            message.BookingId,
            "Confirmed",
            cancellationToken);
    }

    private async Task SendConfirmationEmailAsync(
        PaymentCompletedEvent message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _bookingConfirmationEmailService.SendPaymentCompletedEmailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was confirmed but confirmation email could not be sent to {CustomerEmail}",
                message.BookingId,
                message.CustomerEmail);
        }
    }

    private async Task PublishDashboardActivityAsync(
        PaymentCompletedEvent message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _adminDashboardNotificationService.PublishBookingCompletedAsync(
                message.BookingId,
                message.Amount,
                message.CustomerName,
                message.CompletedAt,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Error publishing dashboard activity for booking {BookingId}",
                message.BookingId);
        }
    }
}
