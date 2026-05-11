using Cinema.Shared.Entities;

namespace Movie.API.Domain.Entities;

public class Movie : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public string? Language { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public static bool TryNormalizeMovieStatus(string? status, out string? normalizedStatus)
    {
        normalizedStatus = null;

        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        normalizedStatus = status.Trim().ToLowerInvariant() switch
        {
            "showing" => MovieStatuses.Showing,
            "comingsoon" => MovieStatuses.ComingSoon,
            "coming-soon" => MovieStatuses.ComingSoon,
            "coming_soon" => MovieStatuses.ComingSoon,
            "archived" => MovieStatuses.Archived,
            _ => null
        };

        return normalizedStatus is not null;
    }
}


