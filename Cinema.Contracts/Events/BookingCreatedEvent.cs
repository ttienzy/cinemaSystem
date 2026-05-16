namespace Cinema.Contracts.Events;

public class BookingCreatedEvent
{
    public Guid BookingId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid ShowtimeId { get; init; }
    public List<Guid> SeatIds { get; init; } = new();
    public decimal TotalPrice { get; init; }
    public DateTime BookingDate { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
