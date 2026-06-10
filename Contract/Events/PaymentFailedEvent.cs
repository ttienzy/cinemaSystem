namespace Cinema.Contracts.Events;

public class PaymentFailedEvent
{
    public Guid CorrelationId { get; set; }
    public Guid BookingId { get; set; }
    public Guid PaymentId { get; set; }
    public long Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
