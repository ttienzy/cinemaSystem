using Payment.API.Entities;

namespace Payment.API.Messaging.EventPublishers;

public interface IPaymentIntegrationEventPublisher
{
    Task PublishPaymentCompletedAsync(PaymentEntity payment, string? transactionId, DateTime completedAt);
    Task PublishPaymentFailedAsync(PaymentEntity payment, string reason, DateTime failedAt);
}


