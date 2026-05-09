using Booking.API.Infrastructure.Persistence;
using Booking.API.Domain.Entities;
using BookingEntity = Booking.API.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;

namespace Booking.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Booking entity
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(
        BookingDbContext context,
        ILogger<BookingRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BookingEntity?> GetByIdWithSeatsAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.BookingSeats)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<BookingEntity>> GetByIdsWithSeatsAsync(IEnumerable<Guid> ids)
    {
        var bookingIds = ids.Distinct().ToList();
        if (bookingIds.Count == 0)
        {
            return new List<BookingEntity>();
        }

        return await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => bookingIds.Contains(b.Id))
            .ToListAsync();
    }

    public async Task<List<BookingEntity>> GetByUserIdAsync(string userId)
    {
        return await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<List<BookingEntity>> GetByShowtimeIdAsync(Guid showtimeId)
    {
        return await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.ShowtimeId == showtimeId)
            .ToListAsync();
    }

    public async Task<BookingEntity> CreateAsync(BookingEntity booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created booking {BookingId} for user {UserId}",
            booking.Id, booking.UserId);

        return booking;
    }

    public async Task<BookingEntity> UpdateAsync(BookingEntity booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated booking {BookingId} to status {Status}",
            booking.Id, booking.Status);

        return booking;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var booking = await GetByIdAsync(id);
        if (booking == null)
        {
            return false;
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted booking {BookingId}", id);

        return true;
    }

    public async Task<List<BookingEntity>> GetExpiredBookingsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.Status == BookingStatus.Pending
                     && b.ExpiresAt.HasValue
                     && b.ExpiresAt.Value < now)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Bookings.AnyAsync(b => b.Id == id);
    }

    public async Task<int> CountBookedSeatsForShowtimeAsync(Guid showtimeId)
    {
        return await _context.BookingSeats
            .Where(bs => bs.Booking.ShowtimeId == showtimeId
                      && (bs.Booking.Status == BookingStatus.Confirmed
                       || bs.Booking.Status == BookingStatus.CheckedIn))
            .CountAsync();
    }

    public async Task<Dictionary<Guid, int>> GetBookedSeatCountsByShowtimeIdsAsync(IEnumerable<Guid> showtimeIds)
    {
        var ids = showtimeIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await _context.BookingSeats
            .Where(bs => ids.Contains(bs.Booking.ShowtimeId)
                      && (bs.Booking.Status == BookingStatus.Confirmed
                       || bs.Booking.Status == BookingStatus.CheckedIn))
            .GroupBy(bs => bs.Booking.ShowtimeId)
            .Select(group => new
            {
                ShowtimeId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(x => x.ShowtimeId, x => x.Count);
    }
}



