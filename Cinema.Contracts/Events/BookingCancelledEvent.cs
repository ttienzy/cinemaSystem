namespace Cinema.Contracts.Events;

public class BookingCancelledEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public Guid BookingId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = new();
    public bool NeedsRefund { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
