using System.ComponentModel.DataAnnotations;

namespace Booking.API.Application.DTOs.Requests;

/// <summary>
/// Request to unlock previously locked seats
/// Note: UserId will be extracted from JWT token by the server
/// </summary>
public class UnlockSeatsRequest
{
    [Required]
    public Guid ShowtimeId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> SeatIds { get; set; } = new();

    // UserId is set by server from JWT token, not from client
    public string UserId { get; set; } = string.Empty;
}


