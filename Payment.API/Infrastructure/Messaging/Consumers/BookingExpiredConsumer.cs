using Cinema.Contracts.Events;
using MassTransit;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.Consumers;

public class BookingExpiredConsumer : IConsumer<BookingExpiredEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BookingExpiredConsumer> _logger;

    public BookingExpiredConsumer(
        IPaymentService paymentService,
        ILogger<BookingExpiredConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingExpiredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed BookingExpiredEvent for booking {BookingId}, expired at {ExpiredAt}",
            message.BookingId,
            message.ExpiredAt);

        var paymentResult = await _paymentService.GetPaymentByBookingIdAsync(message.BookingId);
        if (!paymentResult.Success || paymentResult.Data is null)
        {
            _logger.LogWarning(
                "Payment not found for expired booking {BookingId}",
                message.BookingId);
            return;
        }

        var payment = paymentResult.Data;
        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
        {
            _logger.LogInformation(
                "Skipping expiration for payment {PaymentId} because status is {Status}",
                payment.Id,
                payment.Status);
            return;
        }

        await _paymentService.UpdatePaymentStatusAsync(
            payment.Id,
            PaymentStatus.Cancelled,
            gatewayMetadata: $"Booking expired at {message.ExpiredAt:O}");
    }
}
