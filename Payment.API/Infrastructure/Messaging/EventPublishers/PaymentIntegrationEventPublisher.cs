using Cinema.Contracts.Events;
using MassTransit;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.EventPublishers;

public class PaymentIntegrationEventPublisher : IPaymentIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentIntegrationEventPublisher> _logger;

    public PaymentIntegrationEventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentIntegrationEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishPaymentCompletedAsync(
        PaymentEntity payment,
        string? transactionId,
        DateTime completedAt)
    {
        _logger.LogInformation(
            "Publishing PaymentCompletedEvent for payment {PaymentId}",
            payment.Id);

        await _publishEndpoint.Publish(new PaymentCompletedEvent
        {
            CorrelationId = payment.BookingId,
            BookingId = payment.BookingId,
            PaymentId = payment.Id,
            TransactionId = transactionId ?? string.Empty,
            Amount = payment.Amount,
            CompletedAt = completedAt,
            CustomerEmail = payment.CustomerEmail,
            CustomerName = payment.CustomerName
        });
    }

    public async Task PublishPaymentFailedAsync(PaymentEntity payment, string reason, DateTime failedAt)
    {
        _logger.LogInformation(
            "Publishing PaymentFailedEvent for payment {PaymentId}",
            payment.Id);

        await _publishEndpoint.Publish(new PaymentFailedEvent
        {
            CorrelationId = payment.BookingId,
            BookingId = payment.BookingId,
            PaymentId = payment.Id,
            Reason = reason,
            FailedAt = failedAt
        });
    }
}
