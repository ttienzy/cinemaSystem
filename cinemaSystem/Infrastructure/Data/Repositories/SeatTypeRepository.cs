using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class SeatTypeRepository(BookingContext context) : ISeatTypeRepository
    {
        public async Task<SeatType?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<SeatType>().FindAsync([id], ct);

        public async Task<List<SeatType>> GetAllAsync(CancellationToken ct = default)
            => await context.Set<SeatType>()
                .OrderBy(s => s.TypeName)
                .ToListAsync(ct);

        public async Task AddAsync(SeatType seatType, CancellationToken ct = default)
            => await context.Set<SeatType>().AddAsync(seatType, ct);

        public void Update(SeatType seatType)
            => context.Set<SeatType>().Update(seatType);

        public void Delete(SeatType seatType)
            => context.Set<SeatType>().Remove(seatType);
    }
}
