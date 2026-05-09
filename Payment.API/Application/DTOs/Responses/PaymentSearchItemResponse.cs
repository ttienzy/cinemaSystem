using Payment.API.Domain.Entities;

namespace Payment.API.Application.DTOs.Responses;

public class PaymentSearchItemResponse
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public long Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
