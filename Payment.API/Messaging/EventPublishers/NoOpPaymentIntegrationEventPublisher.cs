using Payment.API.Entities;

namespace Payment.API.Messaging.EventPublishers;

public class NoOpPaymentIntegrationEventPublisher : IPaymentIntegrationEventPublisher
{
    private readonly ILogger<NoOpPaymentIntegrationEventPublisher> _logger;

    public NoOpPaymentIntegrationEventPublisher(ILogger<NoOpPaymentIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishPaymentCompletedAsync(PaymentEntity payment, string? transactionId, DateTime completedAt)
    {
        // RabbitMQ/MassTransit publishing is disabled for the current Payment refactor.
        _logger.LogDebug("PaymentCompletedEvent publishing skipped for payment {PaymentId}", payment.Id);
        return Task.CompletedTask;
    }

    public Task PublishPaymentFailedAsync(PaymentEntity payment, string reason, DateTime failedAt)
    {
        // RabbitMQ/MassTransit publishing is disabled for the current Payment refactor.
        _logger.LogDebug("PaymentFailedEvent publishing skipped for payment {PaymentId}", payment.Id);
        return Task.CompletedTask;
    }
}
