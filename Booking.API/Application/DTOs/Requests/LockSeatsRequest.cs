using System.ComponentModel.DataAnnotations;

namespace Booking.API.Application.DTOs.Requests;

/// <summary>
/// Request to temporarily lock seats for a user
/// </summary>
public class LockSeatsRequest
{
    [Required]
    public Guid ShowtimeId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> SeatIds { get; set; } = new();

    [Required]
    public string UserId { get; set; } = string.Empty;
}


