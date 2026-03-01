using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class TimeSlotRepository(BookingContext context) : ITimeSlotRepository
    {
        public async Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<TimeSlot>().FindAsync([id], ct);

        public async Task<List<TimeSlot>> GetAllAsync(CancellationToken ct = default)
            => await context.Set<TimeSlot>()
                .OrderBy(t => t.StartTime)
                .ToListAsync(ct);

        public async Task AddAsync(TimeSlot timeSlot, CancellationToken ct = default)
            => await context.Set<TimeSlot>().AddAsync(timeSlot, ct);

        public void Update(TimeSlot timeSlot)
            => context.Set<TimeSlot>().Update(timeSlot);

        public void Delete(TimeSlot timeSlot)
            => context.Set<TimeSlot>().Remove(timeSlot);
    }
}
