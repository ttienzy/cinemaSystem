namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Payment entity from Payment.API
/// </summary>
public class PaymentEntityDto
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
    public int Status { get; set; }  // ✅ FIXED: Changed from string to int (enum value)
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
}

/// <summary>
/// Payment status enum (matches Payment.API)
/// </summary>
public enum PaymentStatusDto
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}
