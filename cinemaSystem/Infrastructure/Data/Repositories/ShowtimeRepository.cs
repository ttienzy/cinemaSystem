using Application.Common.Interfaces.Persistence;
using Domain.Entities.ShowtimeAggregate;
using Domain.Entities.ShowtimeAggregate.Enum;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Showtime aggregate repository — implements new CQRS interface.
    /// Old reporting queries (performance dashboard, featured) remain in Data/Services/ShowtimeService.
    /// </summary>
    public class ShowtimeRepository(BookingContext context) : IShowtimeRepository
    {
        public async Task<Showtime?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Showtimes.FindAsync([id], ct);

        public async Task<Showtime?> GetByIdWithPricingAsync(Guid id, CancellationToken ct = default)
            => await context.Showtimes
                .Include(s => s.ShowtimePricings)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<List<Showtime>> GetByCinemaAndDateAsync(
            Guid cinemaId, DateTime date, CancellationToken ct = default)
            => await context.Showtimes
                .Where(s => s.CinemaId == cinemaId && s.ShowDate.Date == date.Date)
                .Include(s => s.ShowtimePricings)
                .OrderBy(s => s.ActualStartTime)
                .ToListAsync(ct);

        public async Task<bool> HasOverlappingAsync(
        Guid screenId,
        DateTime showDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int cleaningOffsetMinutes = 0,
        CancellationToken ct = default)
        {
            var startDateTime = showDate.Date.Add(startTime.ToTimeSpan());
            var endDateTime = showDate.Date.Add(endTime.ToTimeSpan());

            return await context.Showtimes
                .AnyAsync(s =>
                    s.ScreenId == screenId
                    && s.ShowDate.Date == showDate.Date
                    && s.Status != ShowtimeStatus.Cancelled
                    && s.ActualStartTime.AddMinutes(-cleaningOffsetMinutes) < endDateTime
                    && s.ActualEndTime.AddMinutes(cleaningOffsetMinutes) > startDateTime,
                    ct);
        }

        public async Task<bool> HasOverlappingExcludingAsync(
        Guid screenId,
        DateTime showDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid excludeId,
        CancellationToken ct = default)
        {
            var startDateTime = showDate.Date.Add(startTime.ToTimeSpan());
            var endDateTime = showDate.Date.Add(endTime.ToTimeSpan());

            return await context.Showtimes
                .AnyAsync(s =>
                    s.ScreenId == screenId
                    && s.Id != excludeId
                    && s.ShowDate.Date == showDate.Date
                    && s.Status != ShowtimeStatus.Cancelled
                    && s.ActualStartTime < endDateTime
                    && s.ActualEndTime > startDateTime,
                    ct);
        }

        public async Task AddAsync(Showtime showtime, CancellationToken ct = default)
            => await context.Showtimes.AddAsync(showtime, ct);

        public void Update(Showtime showtime)
            => context.Showtimes.Update(showtime);

        public void Delete(Showtime showtime)
            => context.Showtimes.Remove(showtime);

        public async Task<List<Domain.Entities.SharedAggregates.SeatType>> GetSeatTypesAsync(CancellationToken ct = default)
            => await context.SeatTypes.ToListAsync(ct);

        public async Task<Domain.Entities.SharedAggregates.PricingTier?> GetPricingTierAsync(Guid id, CancellationToken ct = default)
            => await context.PricingTiers.FindAsync([id], ct);

        public async Task<List<Domain.Entities.SharedAggregates.TimeSlot>> GetTimeSlotsAsync(CancellationToken ct = default)
            => await context.TimeSlots.Where(t => t.IsActive).ToListAsync(ct);

        public async Task<Domain.Entities.SharedAggregates.TimeSlot?> GetTimeSlotAsync(Guid id, CancellationToken ct = default)
            => await context.TimeSlots.FindAsync([id], ct);

        public IQueryable<Showtime> GetQueryable()
            => context.Showtimes;

    }
}
