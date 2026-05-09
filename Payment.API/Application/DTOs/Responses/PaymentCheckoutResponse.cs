namespace Payment.API.Application.DTOs.Responses;

public class PaymentCheckoutResponse
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string CheckoutFormAction { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> CheckoutFormFields { get; set; }
        = new Dictionary<string, string>();
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public long Amount { get; set; }
    public DateTime? ExpiresAt { get; set; }
}


