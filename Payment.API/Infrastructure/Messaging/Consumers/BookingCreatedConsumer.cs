using Cinema.Contracts.Events;
using Cinema.EventBus.Events;
using MassTransit;
using Payment.API.Infrastructure.Messaging.EventHandlers;

namespace Payment.API.Infrastructure.Messaging.Consumers;

public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly BookingCreatedIntegrationEventHandler _handler;
    private readonly ILogger<BookingCreatedConsumer> _logger;

    public BookingCreatedConsumer(
        BookingCreatedIntegrationEventHandler handler,
        ILogger<BookingCreatedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed BookingCreatedEvent for booking {BookingId}, message {MessageId}",
            message.BookingId,
            context.MessageId);

        await _handler.Handle(new BookingCreatedIntegrationEvent(
            message.BookingId,
            message.UserId,
            message.ShowtimeId,
            message.SeatIds,
            message.TotalPrice,
            message.BookingDate,
            message.CustomerEmail,
            message.CustomerPhone,
            message.CustomerName));
    }
}
