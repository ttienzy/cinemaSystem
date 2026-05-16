using Cinema.Contracts.Events;
using Cinema.EventBus.Events;
using MassTransit;

namespace Booking.API.Infrastructure.Messaging.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly PaymentFailedIntegrationEventHandler _handler;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        PaymentFailedIntegrationEventHandler handler,
        ILogger<PaymentFailedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed PaymentFailedEvent for booking {BookingId}, message {MessageId}",
            message.BookingId,
            context.MessageId);

        await _handler.Handle(new PaymentFailedIntegrationEvent(
            message.PaymentId,
            message.BookingId,
            message.Reason,
            message.FailedAt));
    }
}
