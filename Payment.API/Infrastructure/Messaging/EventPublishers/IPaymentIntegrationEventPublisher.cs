using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.EventPublishers;

public interface IPaymentIntegrationEventPublisher
{
    void PublishPaymentCompleted(PaymentEntity payment, string? transactionId, DateTime completedAt);
    void PublishPaymentFailed(PaymentEntity payment, string reason, DateTime failedAt);
}


