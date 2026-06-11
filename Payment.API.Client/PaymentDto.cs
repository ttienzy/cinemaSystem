namespace Payment.API.Client;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string OrderDescription { get; set; } = string.Empty;
    public string PaymentGateway { get; set; } = "SePay";
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}
