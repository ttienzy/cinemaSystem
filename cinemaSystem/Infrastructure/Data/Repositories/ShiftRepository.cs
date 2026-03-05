using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository Ca làm — truy xuất và quản lý ca làm theo rạp.
    /// </summary>
    public class ShiftRepository(BookingContext context) : IShiftRepository
    {
        public async Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<Shift>().FindAsync([id], ct);

        public async Task<List<Shift>> GetByCinemaAsync(Guid cinemaId, CancellationToken ct = default)
            => await context.Set<Shift>()
                .Where(s => s.CinemaId == cinemaId)
                .OrderBy(s => s.DefaultStartTime)
                .ToListAsync(ct);

        public async Task AddAsync(Shift shift, CancellationToken ct = default)
            => await context.Set<Shift>().AddAsync(shift, ct);

        public void Update(Shift shift)
            => context.Set<Shift>().Update(shift);

        public void Delete(Shift shift)
            => context.Set<Shift>().Remove(shift);

        public IQueryable<Shift> GetQueryable()
            => context.Set<Shift>().AsQueryable();
    }
}
