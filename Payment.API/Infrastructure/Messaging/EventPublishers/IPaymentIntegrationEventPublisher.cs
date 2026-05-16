using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.EventPublishers;

public interface IPaymentIntegrationEventPublisher
{
    Task PublishPaymentCompletedAsync(PaymentEntity payment, string? transactionId, DateTime completedAt);
    Task PublishPaymentFailedAsync(PaymentEntity payment, string reason, DateTime failedAt);
}


