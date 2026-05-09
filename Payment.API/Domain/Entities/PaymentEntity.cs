using Cinema.Shared.Entities;

namespace Payment.API.Domain.Entities;

/// <summary>
/// Payment aggregate owned by Payment.API.
/// Other services only reference it by BookingId / PaymentId.
/// </summary>
public class PaymentEntity : BaseEntity
{
    public Guid BookingId { get; set; }

    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string OrderDescription { get; set; } = string.Empty;

    public string PaymentGateway { get; set; } = "SePay";
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    // Stores the last gateway payload / metadata for diagnostics and audit.
    public string? GatewayMetadata { get; set; }

    public string? SuccessUrl { get; set; }
    public string? ErrorUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}


