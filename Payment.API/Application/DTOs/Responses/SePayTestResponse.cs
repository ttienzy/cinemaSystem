namespace Payment.API.Application.DTOs.Responses;

/// <summary>
/// Response for SePay test payment - returns checkout URL and form data
/// </summary>
public class SePayTestResponse
{
    /// <summary>
    /// Payment ID (for reference)
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Invoice number (TEST-...)
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// SePay checkout form action URL
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>
    /// Form fields to submit to SePay
    /// </summary>
    public Dictionary<string, string> FormFields { get; set; } = new();

    /// <summary>
    /// Payment amount
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Payment method
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Instructions for manual redirect
    /// </summary>
    public string Instructions { get; set; } = "Use the checkoutUrl and formFields to create a form and submit it to SePay";
}
