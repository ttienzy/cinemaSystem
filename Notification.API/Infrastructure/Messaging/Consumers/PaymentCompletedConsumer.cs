using Cinema.Contracts.Events;
using MassTransit;
using Notification.API.Infrastructure.Notifications;

namespace Notification.API.Infrastructure.Messaging.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IEmailService emailService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Sending payment completion email for booking {BookingId}, payment {PaymentId}",
            message.BookingId,
            message.PaymentId);

        await _emailService.SendPaymentCompletedAsync(new PaymentCompletedEmail
        {
            BookingId = message.BookingId,
            PaymentId = message.PaymentId,
            TransactionId = message.TransactionId,
            Amount = message.Amount,
            CompletedAt = message.CompletedAt,
            CustomerEmail = message.CustomerEmail,
            CustomerName = message.CustomerName
        }, context.CancellationToken);
    }
}
