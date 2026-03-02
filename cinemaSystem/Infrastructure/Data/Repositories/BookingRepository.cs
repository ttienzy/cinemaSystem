using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Booking aggregate repository — implements new CQRS-aligned interface.
    /// All cross-aggregate query reporting stays in old ReadModel repositories (kept in Data/Services).
    /// </summary>
    public class BookingRepository(BookingContext context) : IBookingRepository
    {
        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Bookings
                .Include(b => b.BookingTickets)
                .FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
            => await context.Bookings
                .Include(b => b.BookingTickets)
                .Include(b => b.Payments)
                .Include(b => b.Refunds)
                .FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task<Booking?> GetByBookingCodeAsync(string code, CancellationToken ct = default)
            => await context.Bookings
                .Include(b => b.BookingTickets)
                .FirstOrDefaultAsync(b => b.BookingCode == code, ct);

        public async Task<List<Booking>> GetExpiredPendingAsync(CancellationToken ct = default)
            => await context.Bookings
                .Where(b => b.Status == Domain.Entities.BookingAggregate.Enums.BookingStatus.Pending
                         && b.ExpiresAt < DateTime.UtcNow)
                .Include(b => b.BookingTickets)
                .ToListAsync(ct);

        public async Task<List<Booking>> GetByCustomerAsync(
            Guid customerId, int page, int pageSize, CancellationToken ct = default)
            => await context.Bookings
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BookingTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.BookingTickets)
                .ToListAsync(ct);

        public async Task<Booking?> GetByIdForCheckInAsync(Guid id, CancellationToken ct = default)
            => await context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task AddAsync(Booking booking, CancellationToken ct = default)
            => await context.Bookings.AddAsync(booking, ct);

        public void Update(Booking booking)
            => context.Bookings.Update(booking);

        public IQueryable<Booking> GetQueryable()
            => context.Bookings;
    }
}
