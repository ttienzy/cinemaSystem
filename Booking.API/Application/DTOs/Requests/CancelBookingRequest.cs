using System.ComponentModel.DataAnnotations;

namespace Booking.API.Application.DTOs.Requests;

/// <summary>
/// Request to cancel a booking
/// </summary>
public class CancelBookingRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? CancellationReason { get; set; }
}


