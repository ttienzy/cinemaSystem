namespace Cinema.Contracts.Events;

public record PaymentFailedEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}
