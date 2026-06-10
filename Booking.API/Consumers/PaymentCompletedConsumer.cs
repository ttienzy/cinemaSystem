
using Booking.API.Services;
using Cinema.Contracts.Events;
using MassTransit;

namespace Booking.API.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IBookingService _bookingService;
    //private readonly IAdminDashboardNotificationService _adminDashboardNotificationService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IBookingService bookingService,
        //IAdminDashboardNotificationService adminDashboardNotificationService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _bookingService = bookingService;
        //_adminDashboardNotificationService = adminDashboardNotificationService;
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

                await PublishDashboardActivityAsync(message);
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

    private async Task PublishDashboardActivityAsync(PaymentCompletedEvent message)
    {
        try
        {
            // SignalR dashboard notification is intentionally skipped in the RabbitMQ phase.
            //await _adminDashboardNotificationService.PublishBookingCompletedAsync(
            //    message.BookingId,
            //    message.Amount,
            //    message.CustomerName,
            //    message.CompletedAt);

            _logger.LogInformation(
                "Dashboard activity skipped for completed booking {BookingId}, customer {CustomerName}, amount {Amount}, completed at {CompletedAt}",
                message.BookingId,
                message.CustomerName,
                message.Amount,
                message.CompletedAt);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing dashboard activity for booking {BookingId}",
                message.BookingId);
        }
    }
}
