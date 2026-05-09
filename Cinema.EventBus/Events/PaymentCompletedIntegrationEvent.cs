using Cinema.EventBus.Abstractions;

namespace Cinema.EventBus.Events;

/// <summary>
/// Event published when payment is completed successfully
/// </summary>
public class PaymentCompletedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CompletedAt { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public PaymentCompletedIntegrationEvent()
    {
    }

    public PaymentCompletedIntegrationEvent(
        Guid paymentId,
        Guid bookingId,
        string transactionId,
        decimal amount,
        DateTime completedAt,
        string customerEmail,
        string customerName)
    {
        PaymentId = paymentId;
        BookingId = bookingId;
        TransactionId = transactionId;
        Amount = amount;
        CompletedAt = completedAt;
        CustomerEmail = customerEmail;
        CustomerName = customerName;
    }
}
