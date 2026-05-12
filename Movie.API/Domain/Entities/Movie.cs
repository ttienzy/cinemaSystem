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

    public static Movie Create(
        string title,
        string? description,
        int duration,
        string? language,
        DateTime releaseDate,
        string? posterUrl,
        IEnumerable<Guid> genreIds)
    {
        var movie = new Movie();
        movie.SetBasicInfo(title, description, duration, language, releaseDate, posterUrl);
        movie.SetGenres(genreIds);

        return movie;
    }

    public void UpdateDetails(
        string title,
        string? description,
        int duration,
        string? language,
        DateTime releaseDate,
        string? posterUrl)
    {
        SetBasicInfo(title, description, duration, language, releaseDate, posterUrl);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetGenres(IEnumerable<Guid> genreIds)
    {
        MovieGenres = genreIds
            .Distinct()
            .Select(genreId => new MovieGenre
            {
                MovieId = Id,
                GenreId = genreId
            })
            .ToList();
    }

    public bool HasGenre(Guid genreId)
    {
        return MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId);
    }

    public bool HasShowtimes()
    {
        return Showtimes.Count != 0;
    }

    public string GetStatus(DateTime now)
    {
        if (ReleaseDate.Date > now.Date)
        {
            return MovieStatuses.ComingSoon;
        }

        if (Showtimes.Any(showtime => showtime.EndTime >= now))
        {
            return MovieStatuses.Showing;
        }

        return MovieStatuses.Archived;
    }

    public bool MatchesStatus(string status, DateTime now)
    {
        return string.Equals(GetStatus(now), status, StringComparison.Ordinal);
    }

    public int GetStatusRank(DateTime now)
    {
        return GetMovieStatusRank(GetStatus(now));
    }

    public int GetUpcomingShowtimesCount(DateTime now)
    {
        return Showtimes.Count(showtime => showtime.EndTime >= now);
    }

    public DateTime? GetNextShowtimeAt(DateTime now)
    {
        return Showtimes
            .Where(showtime => showtime.EndTime >= now)
            .OrderBy(showtime => showtime.StartTime)
            .Select(showtime => (DateTime?)showtime.StartTime)
            .FirstOrDefault();
    }

    public DateTime? GetLastShowtimeAt()
    {
        return Showtimes
            .OrderByDescending(showtime => showtime.EndTime)
            .Select(showtime => (DateTime?)showtime.EndTime)
            .FirstOrDefault();
    }

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
    public static int GetMovieStatusRank(string status)
    {
        return status switch
        {
            MovieStatuses.Showing => 0,
            MovieStatuses.ComingSoon => 1,
            MovieStatuses.Archived => 2,
            _ => 3
        };
    }
    public static string DetermineMovieStatus(Movie movie, DateTime now)
    {
        return movie.GetStatus(now);
    }

    private void SetBasicInfo(
        string title,
        string? description,
        int duration,
        string? language,
        DateTime releaseDate,
        string? posterUrl)
    {
        Title = title;
        Description = description;
        Duration = duration;
        Language = language;
        ReleaseDate = releaseDate;
        PosterUrl = posterUrl;
    }
}


