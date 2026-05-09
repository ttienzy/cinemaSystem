namespace Booking.API.Application.DTOs.Responses;

/// <summary>
/// Showtime details for booking response
/// </summary>
public class ShowtimeDetailsDto
{
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public string CinemaHallName { get; set; } = string.Empty;
}


