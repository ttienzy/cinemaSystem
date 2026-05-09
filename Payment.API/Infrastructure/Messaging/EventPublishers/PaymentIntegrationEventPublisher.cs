using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Messaging.EventPublishers;

public class PaymentIntegrationEventPublisher : IPaymentIntegrationEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentIntegrationEventPublisher> _logger;

    public PaymentIntegrationEventPublisher(
        IServiceProvider serviceProvider,
        ILogger<PaymentIntegrationEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void PublishPaymentCompleted(
        PaymentEntity payment,
        string? transactionId,
        DateTime completedAt)
    {
        _logger.LogInformation(
            "Publishing PaymentCompletedIntegrationEvent for payment {PaymentId}",
            payment.Id);

        ResolveEventBus().Publish(new PaymentCompletedIntegrationEvent
        {
            BookingId = payment.BookingId,
            PaymentId = payment.Id,
            TransactionId = transactionId ?? string.Empty,
            Amount = payment.Amount,
            CompletedAt = completedAt,
            CustomerEmail = payment.CustomerEmail,
            CustomerName = payment.CustomerName
        });
    }

    public void PublishPaymentFailed(PaymentEntity payment, string reason, DateTime failedAt)
    {
        _logger.LogInformation(
            "Publishing PaymentFailedIntegrationEvent for payment {PaymentId}",
            payment.Id);

        ResolveEventBus().Publish(new PaymentFailedIntegrationEvent
        {
            BookingId = payment.BookingId,
            PaymentId = payment.Id,
            Reason = reason,
            FailedAt = failedAt
        });
    }

    private IEventBus ResolveEventBus() =>
        _serviceProvider.GetRequiredService<IEventBus>();
}


