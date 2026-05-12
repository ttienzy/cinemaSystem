namespace Movie.API.Application.Mappers;

public static class ShowtimeMapper
{
    public static ShowtimeDto ShowtimeMapToDto(this Showtime showtime, CinemaHallInfo? cinemaHallInfo)
    {
        return new ShowtimeDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo?.CinemaName,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = showtime.GetDurationMinutes(),
            CreatedAt = showtime.CreatedAt
        };
    }

    public static ShowtimeDetailDto ShowtimeDetailMapToDto(this Showtime showtime, CinemaHallInfo? cinemaHallInfo, DateTime now)
    {
        return new ShowtimeDetailDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo?.CinemaName,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = showtime.GetDurationMinutes(),
            CreatedAt = showtime.CreatedAt,
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

    public static ShowtimeConflictItemDto ShowtimeMapToConflictItemDto(this Showtime showtime, int cleaningBufferMinutes)
    {
        return new ShowtimeConflictItemDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            CleaningEndTime = showtime.GetCleaningEndTime(cleaningBufferMinutes)
        };
    }

    public static TimelineShowtimeDto ShowtimeMapToTimelineItemDto(
        this Showtime showtime,
        int cleaningBufferMinutes,
        int totalSeats,
        int bookedSeats,
        DateTime now)
    {
        return new TimelineShowtimeDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            Start = showtime.StartTime,
            End = showtime.EndTime,
            CleaningEnd = showtime.GetCleaningEndTime(cleaningBufferMinutes),
            DurationMinutes = showtime.GetDurationMinutes(),
            CleaningBufferMinutes = cleaningBufferMinutes,
            Price = showtime.Price,
            TotalSeats = totalSeats,
            BookedSeats = bookedSeats,
            OccupancyRate = showtime.GetOccupancyRate(bookedSeats, totalSeats),
            HasBookings = showtime.HasBookings(bookedSeats),
            CanReschedule = showtime.CanReschedule(bookedSeats, now)
        };
    }
}
