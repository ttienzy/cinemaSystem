using Movie.API.Application.DTOs;
using MovieEntity = Movie.API.Domain.Entities.Movie;

namespace Movie.API.Application.Mappers;

public static class MovieMapper
{
    public static MovieDto MovieMapToDto(this MovieEntity movie, DateTime now)
    {
        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = movie.GetStatus(now),
            CreatedAt = movie.CreatedAt,
            Genres = MapGenres(movie)
        };
    }

    public static MovieDetailDto MovieMapToDetailDto(this MovieEntity movie, DateTime now)
    {
        return new MovieDetailDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = movie.GetStatus(now),
            CreatedAt = movie.CreatedAt,
            Genres = MapGenres(movie),
            Showtimes = movie.Showtimes.Select(showtime => new ShowtimeDto
            {
                Id = showtime.Id,
                MovieId = showtime.MovieId,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                Price = showtime.Price,
                CinemaHallId = showtime.CinemaHallId,
                CreatedAt = showtime.CreatedAt,
                MovieTitle = movie.Title,
                DurationMinutes = movie.Duration
            }).ToList()
        };
    }

    public static MovieAdminListItemDto MovieMapToAdminListItemDto(this MovieEntity movie, DateTime now)
    {
        return new MovieAdminListItemDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = movie.GetStatus(now),
            CreatedAt = movie.CreatedAt,
            Genres = MapGenres(movie),
            TotalShowtimes = movie.Showtimes.Count,
            UpcomingShowtimesCount = movie.GetUpcomingShowtimesCount(now),
            NextShowtimeAt = movie.GetNextShowtimeAt(now),
            LastShowtimeAt = movie.GetLastShowtimeAt()
        };
    }

    private static List<GenreDto> MapGenres(MovieEntity movie)
    {
        return movie.MovieGenres
            .Where(movieGenre => movieGenre.Genre is not null)
            .Select(movieGenre => new GenreDto
            {
                Id = movieGenre.Genre!.Id,
                Name = movieGenre.Genre.Name
            })
            .ToList();
    }
}
