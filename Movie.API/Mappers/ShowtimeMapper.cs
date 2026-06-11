using Movie.API.Client;
using Movie.API.Entities;
using CinemaHallDetailDto = Cinema.API.Client.CinemaHallDetailDto;
using CinemaHallDto = Cinema.API.Client.CinemaHallDto;

namespace Movie.API.Mappers;

public static class ShowtimeMapper
{
    public static ShowtimeDto ShowtimeMapToDto(this Showtime showtime, CinemaHallDto? cinemaHallInfo)
    {
        return new ShowtimeDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo is CinemaHallDetailDto detail ? detail.CinemaName : null,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = showtime.GetDurationMinutes(),
        };
    }

    public static ShowtimeDetailDto ShowtimeDetailMapToDto(this Showtime showtime, CinemaHallDto? cinemaHallInfo, DateTime now)
    {
        return new ShowtimeDetailDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo is CinemaHallDetailDto detail ? detail.CinemaName : null,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = showtime.GetDurationMinutes(),
            Movie = showtime.Movie.MovieMapToDto(now),
            TotalSeats = cinemaHallInfo?.TotalSeats ?? 0,
            AvailableSeats = cinemaHallInfo?.TotalSeats ?? 0
        };
    }

    public static ShowtimeLookupItemDto ShowtimeMapToLookupDto(this Showtime showtime)
    {
        return new ShowtimeLookupItemDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            PosterUrl = showtime.Movie.PosterUrl,
            CinemaHallId = showtime.CinemaHallId,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price
        };
    }

}
