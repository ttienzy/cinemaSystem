using System.Text.Json.Serialization;

namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Movie data from Movie.API
/// </summary>
public class MovieDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("duration")]
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = string.Empty;
}


