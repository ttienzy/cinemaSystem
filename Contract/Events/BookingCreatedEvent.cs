namespace Cinema.Contracts.Events;

public class BookingCreatedEvent
{
    public Guid CorrelationId { get; set; }
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
