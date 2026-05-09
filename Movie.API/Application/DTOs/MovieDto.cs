namespace Movie.API.Application.DTOs;

public class MovieDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public string? Language { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<GenreDto> Genres { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class MovieDetailDto : MovieDto
{
    public List<ShowtimeDto> Showtimes { get; set; } = new();
}

public class GenreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


