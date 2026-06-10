namespace Cinema.Contracts.Events;

public class PaymentCompletedEvent
{
    public Guid CorrelationId { get; set; }
    public Guid BookingId { get; set; }
    public Guid PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
