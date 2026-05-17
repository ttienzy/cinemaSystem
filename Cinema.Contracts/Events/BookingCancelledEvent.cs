namespace Cinema.Contracts.Events;

public record BookingCancelledEvent : IntegrationEvent
{
    public Guid BookingId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = [];
    public bool NeedsRefund { get; init; }
    public string Reason { get; init; } = string.Empty;
}
