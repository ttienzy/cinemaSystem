namespace Cinema.Contracts.Events;

public record BookingExpiredEvent : IntegrationEvent
{
    public Guid BookingId { get; init; }
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = [];
    public DateTime ExpiredAt { get; init; }
}
