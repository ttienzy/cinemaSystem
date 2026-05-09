using System.ComponentModel.DataAnnotations;

namespace Booking.API.Application.DTOs.Requests;

/// <summary>
/// Request to create a new booking
/// Note: UserId will be extracted from JWT token by the server
/// </summary>
public class CreateBookingRequest
{
    // UserId is set by server from JWT token, not from client
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid ShowtimeId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one seat must be selected")]
    public List<Guid> SeatIds { get; set; } = new();

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string ContactName { get; set; } = string.Empty;
}


