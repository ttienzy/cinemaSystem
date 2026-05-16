namespace Cinema.Contracts.Events;

public class PaymentFailedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
