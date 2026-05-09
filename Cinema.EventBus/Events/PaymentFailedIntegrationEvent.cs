using Cinema.EventBus.Abstractions;

namespace Cinema.EventBus.Events;

/// <summary>
/// Event published when payment fails
/// </summary>
public class PaymentFailedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }

    public PaymentFailedIntegrationEvent()
    {
    }

    public PaymentFailedIntegrationEvent(
        Guid paymentId,
        Guid bookingId,
        string reason,
        DateTime failedAt)
    {
        PaymentId = paymentId;
        BookingId = bookingId;
        Reason = reason;
        FailedAt = failedAt;
    }
}
