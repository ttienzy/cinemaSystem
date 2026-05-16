namespace Cinema.Contracts.Events;

public class BookingExpiredEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public Guid BookingId { get; init; }
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = new();
    public DateTime ExpiredAt { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
