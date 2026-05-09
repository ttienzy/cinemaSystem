namespace Movie.API.Application.DTOs;

public class ShowtimeDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public Guid CinemaHallId { get; set; }
    public string? CinemaHallName { get; set; }
    public string? CinemaName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShowtimeDetailDto : ShowtimeDto
{
    public MovieDto Movie { get; set; } = null!;
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
}


