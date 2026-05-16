using Cinema.Contracts.Events;
using Cinema.EventBus.Events;
using MassTransit;

namespace Booking.API.Infrastructure.Messaging.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly PaymentCompletedIntegrationEventHandler _handler;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        PaymentCompletedIntegrationEventHandler handler,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed PaymentCompletedEvent for booking {BookingId}, message {MessageId}",
            message.BookingId,
            context.MessageId);

        await _handler.Handle(new PaymentCompletedIntegrationEvent(
            message.PaymentId,
            message.BookingId,
            message.TransactionId,
            message.Amount,
            message.CompletedAt,
            message.CustomerEmail,
            message.CustomerName));
    }
}
