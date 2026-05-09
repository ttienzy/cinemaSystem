namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Payment checkout response from Payment.API
/// </summary>
public class PaymentCheckoutDto
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string CheckoutFormAction { get; set; } = string.Empty;
    public Dictionary<string, string> CheckoutFormFields { get; set; } = new();
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public long Amount { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
