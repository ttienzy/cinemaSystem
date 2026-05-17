namespace Cinema.Contracts.Events;

public record BookingCreatedEvent : IntegrationEvent
{
    public Guid BookingId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = [];
    public decimal TotalPrice { get; init; }
    public DateTime BookingDate { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
}
