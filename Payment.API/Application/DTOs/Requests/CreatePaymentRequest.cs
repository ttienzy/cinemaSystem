using System.ComponentModel.DataAnnotations;

namespace Payment.API.Application.DTOs.Requests;

public class CreatePaymentRequest
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    [Range(10000, long.MaxValue, ErrorMessage = "Amount must be at least 10,000 VND to meet payment gateway requirements")]
    public long Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string OrderDescription { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string CustomerName { get; set; } = string.Empty;

    public string? SuccessUrl { get; set; }
    public string? ErrorUrl { get; set; }
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Payment method: BANK_TRANSFER (default), CARD, NAPAS_BANK_TRANSFER
    /// </summary>
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
}


