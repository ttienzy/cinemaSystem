using System.ComponentModel.DataAnnotations;

namespace Movie.API.Application.DTOs;

public class ShowtimeLookupRequest
{
    [Required]
    public List<Guid> ShowtimeIds { get; set; } = new();
}

public class ShowtimeLookupItemDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public Guid CinemaHallId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
}
