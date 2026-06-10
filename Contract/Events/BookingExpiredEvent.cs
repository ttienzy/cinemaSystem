namespace Cinema.Contracts.Events;

public class BookingExpiredEvent
{
    public Guid CorrelationId { get; set; }
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTime ExpiredAt { get; set; }
}
