using Cinema.Contracts.Events;
using MassTransit;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.Consumers;

public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(
        IPaymentService paymentService,
        ILogger<BookingCancelledConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed BookingCancelledEvent for booking {BookingId}, needs refund: {NeedsRefund}",
            message.BookingId,
            message.NeedsRefund);

        var paymentResult = await _paymentService.GetPaymentByBookingIdAsync(message.BookingId);
        if (!paymentResult.Success || paymentResult.Data is null)
        {
            _logger.LogWarning(
                "Payment not found for cancelled booking {BookingId}. Message will be retried.",
                message.BookingId);

            throw new InvalidOperationException(
                $"Payment not found for cancelled booking {message.BookingId}");
        }

        var payment = paymentResult.Data;
        if (payment.Status is PaymentStatus.Cancelled or PaymentStatus.Failed)
        {
            _logger.LogInformation(
                "Payment {PaymentId} for booking {BookingId} is already {Status}",
                payment.Id,
                message.BookingId,
                payment.Status);
            return;
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            _logger.LogWarning(
                "Booking {BookingId} was cancelled after payment {PaymentId} completed. Refund workflow is required.",
                message.BookingId,
                payment.Id);
            return;
        }

        var updateResult = await _paymentService.UpdatePaymentStatusAsync(
            payment.Id,
            PaymentStatus.Cancelled,
            gatewayMetadata: $"Booking cancelled: {message.Reason}");

        if (!updateResult.Success)
        {
            throw new InvalidOperationException(
                $"Failed to cancel payment {payment.Id} for booking {message.BookingId}: {updateResult.Message}");
        }

        _logger.LogInformation(
            "Cancelled payment {PaymentId} for cancelled booking {BookingId}",
            payment.Id,
            message.BookingId);
    }
}
