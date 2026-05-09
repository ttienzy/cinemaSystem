using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Payment.API.Application.DTOs.Requests;

/// <summary>
/// Test request for SePay payment gateway - allows custom parameters for testing
/// </summary>
public class SePayTestRequest
{
    /// <summary>
    /// Payment amount in VND (minimum 10,000 VND)
    /// </summary>
    [Required]
    [Range(10000, 100000000, ErrorMessage = "Amount must be between 10,000 and 100,000,000 VND")]
    [DefaultValue(100000)]
    public long Amount { get; set; } = 100000;

    /// <summary>
    /// Order description
    /// </summary>
    [Required]
    [MaxLength(500)]
    [DefaultValue("Test payment for Cinema Booking System")]
    public string OrderDescription { get; set; } = "Test payment for Cinema Booking System";

    /// <summary>
    /// Customer email
    /// </summary>
    [Required]
    [EmailAddress]
    [DefaultValue("test@example.com")]
    public string CustomerEmail { get; set; } = "test@example.com";

    /// <summary>
    /// Customer phone number
    /// </summary>
    [Required]
    [Phone]
    [DefaultValue("0123456789")]
    public string CustomerPhone { get; set; } = "0123456789";

    /// <summary>
    /// Customer name
    /// </summary>
    [Required]
    [MinLength(2)]
    [DefaultValue("Test User")]
    public string CustomerName { get; set; } = "Test User";

    /// <summary>
    /// Payment method: BANK_TRANSFER (default), CARD, NAPAS_BANK_TRANSFER
    /// </summary>
    [Required]
    [DefaultValue("BANK_TRANSFER")]
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";

    /// <summary>
    /// Success callback URL (optional - uses default if not provided)
    /// </summary>
    [DefaultValue("https://your-ngrok-url.ngrok-free.dev/api/payments/callback/success")]
    public string? SuccessUrl { get; set; }

    /// <summary>
    /// Error callback URL (optional - uses default if not provided)
    /// </summary>
    [DefaultValue("https://your-ngrok-url.ngrok-free.dev/api/payments/callback/error")]
    public string? ErrorUrl { get; set; }

    /// <summary>
    /// Cancel callback URL (optional - uses default if not provided)
    /// </summary>
    [DefaultValue("https://your-ngrok-url.ngrok-free.dev/api/payments/callback/cancel")]
    public string? CancelUrl { get; set; }
}
