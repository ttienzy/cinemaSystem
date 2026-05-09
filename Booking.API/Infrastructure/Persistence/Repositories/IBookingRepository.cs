using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository interface for Booking entity operations
/// </summary>
public interface IBookingRepository
{
    Task<BookingEntity?> GetByIdAsync(Guid id);
    Task<BookingEntity?> GetByIdWithSeatsAsync(Guid id);
    Task<List<BookingEntity>> GetByIdsWithSeatsAsync(IEnumerable<Guid> ids);
    Task<List<BookingEntity>> GetByUserIdAsync(string userId);
    Task<List<BookingEntity>> GetByShowtimeIdAsync(Guid showtimeId);
    Task<BookingEntity> CreateAsync(BookingEntity booking);
    Task<BookingEntity> UpdateAsync(BookingEntity booking);
    Task<bool> DeleteAsync(Guid id);
    Task<List<BookingEntity>> GetExpiredBookingsAsync();
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountBookedSeatsForShowtimeAsync(Guid showtimeId);
    Task<Dictionary<Guid, int>> GetBookedSeatCountsByShowtimeIdsAsync(IEnumerable<Guid> showtimeIds);
}



