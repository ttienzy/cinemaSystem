using System.Text.Json.Serialization;

namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Showtime data from Movie.API
/// </summary>
public class ShowtimeDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid CinemaHallId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}


