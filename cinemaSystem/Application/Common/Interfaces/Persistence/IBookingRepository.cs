using Domain.Entities.BookingAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<Booking?> GetByBookingCodeAsync(string code, CancellationToken ct = default);
        Task<List<Booking>> GetExpiredPendingAsync(CancellationToken ct = default);
        Task<List<Booking>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default);
        Task<Booking?> GetByIdForCheckInAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Booking booking, CancellationToken ct = default);
        void Update(Booking booking);
    }
}
