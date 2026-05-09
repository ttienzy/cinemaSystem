using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure.Persistence;
using Movie.API.Domain.Entities;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public class ShowtimeRepository : IShowtimeRepository
{
    private readonly MovieDbContext _context;

    public ShowtimeRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<Showtime?> GetByIdAsync(Guid id)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .ThenInclude(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Showtime>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var showtimeIds = ids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (showtimeIds.Count == 0)
        {
            return [];
        }

        return await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => showtimeIds.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<List<Showtime>> GetByMovieIdAsync(Guid movieId)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => s.MovieId == movieId && s.StartTime >= DateTime.UtcNow)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<Showtime>> GetByCinemaHallIdAsync(Guid cinemaHallId)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => s.CinemaHallId == cinemaHallId && s.StartTime >= DateTime.UtcNow)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<Showtime>> GetByCinemaHallIdsAndTimeRangeAsync(IEnumerable<Guid> cinemaHallIds, DateTime from, DateTime to)
    {
        var hallIds = cinemaHallIds.Distinct().ToList();
        if (hallIds.Count == 0)
        {
            return [];
        }

        return await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => hallIds.Contains(s.CinemaHallId)
                     && s.StartTime < to
                     && s.EndTime > from)
            .OrderBy(s => s.CinemaHallId)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<Showtime>> GetByTimeRangeAsync(DateTime from, DateTime to)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .Where(s => s.StartTime < to && s.EndTime > from)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Showtime> CreateAsync(Showtime showtime)
    {
        _context.Showtimes.Add(showtime);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(showtime.Id) ?? showtime;
    }

    public async Task<Showtime?> UpdateAsync(Guid id, Showtime showtime)
    {
        var existing = await _context.Showtimes.FindAsync(id);
        if (existing == null) return null;

        existing.StartTime = showtime.StartTime;
        existing.EndTime = showtime.EndTime;
        existing.Price = showtime.Price;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var showtime = await _context.Showtimes.FindAsync(id);
        if (showtime == null) return false;

        _context.Showtimes.Remove(showtime);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasOverlappingShowtimeAsync(
        Guid cinemaHallId,
        DateTime startTime,
        DateTime endTime,
        int cleaningBufferMinutes,
        Guid? excludeShowtimeId = null)
    {
        return await BuildConflictQuery(cinemaHallId, startTime, endTime, cleaningBufferMinutes, excludeShowtimeId)
            .AnyAsync();
    }

    public async Task<List<Showtime>> GetUpcomingShowtimesAsync(DateTime fromDate, int count)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .ThenInclude(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Where(s => s.StartTime >= fromDate)
            .OrderBy(s => s.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Showtime>> GetConflictingShowtimesAsync(
        Guid cinemaHallId,
        DateTime startTime,
        DateTime endTime,
        int cleaningBufferMinutes,
        Guid? excludeShowtimeId = null)
    {
        return await BuildConflictQuery(cinemaHallId, startTime, endTime, cleaningBufferMinutes, excludeShowtimeId)
            .Include(s => s.Movie)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    private IQueryable<Showtime> BuildConflictQuery(
        Guid cinemaHallId,
        DateTime startTime,
        DateTime endTime,
        int cleaningBufferMinutes,
        Guid? excludeShowtimeId)
    {
        var query = _context.Showtimes
            .Where(s => s.CinemaHallId == cinemaHallId);

        if (excludeShowtimeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeShowtimeId.Value);
        }

        return query.Where(s =>
            startTime < s.EndTime.AddMinutes(cleaningBufferMinutes) &&
            endTime > s.StartTime);
    }
}



