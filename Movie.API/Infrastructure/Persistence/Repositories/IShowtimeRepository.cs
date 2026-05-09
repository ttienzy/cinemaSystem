using Movie.API.Domain.Entities;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public interface IShowtimeRepository
{
    Task<Showtime?> GetByIdAsync(Guid id);
    Task<List<Showtime>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<List<Showtime>> GetByMovieIdAsync(Guid movieId);
    Task<List<Showtime>> GetByCinemaHallIdAsync(Guid cinemaHallId);
    Task<List<Showtime>> GetByCinemaHallIdsAndTimeRangeAsync(IEnumerable<Guid> cinemaHallIds, DateTime from, DateTime to);
    Task<List<Showtime>> GetByTimeRangeAsync(DateTime from, DateTime to);
    Task<Showtime> CreateAsync(Showtime showtime);
    Task<Showtime?> UpdateAsync(Guid id, Showtime showtime);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HasOverlappingShowtimeAsync(Guid cinemaHallId, DateTime startTime, DateTime endTime, int cleaningBufferMinutes, Guid? excludeShowtimeId = null);
    Task<List<Showtime>> GetConflictingShowtimesAsync(Guid cinemaHallId, DateTime startTime, DateTime endTime, int cleaningBufferMinutes, Guid? excludeShowtimeId = null);
    Task<List<Showtime>> GetUpcomingShowtimesAsync(DateTime fromDate, int count);
}



