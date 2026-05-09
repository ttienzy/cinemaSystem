namespace Payment.API.Application.DTOs.SePay;

public class SePayCheckoutResult
{
    public string CheckoutFormAction { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> CheckoutFormFields { get; set; }
        = new Dictionary<string, string>();
    public string OrderInvoiceNumber { get; set; } = string.Empty;
}


