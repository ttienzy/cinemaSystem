using Domain.Entities.ShowtimeAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IShowtimeRepository
    {
        Task<Showtime?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Showtime?> GetByIdWithPricingAsync(Guid id, CancellationToken ct = default);
        Task<List<Showtime>> GetByCinemaAndDateAsync(Guid cinemaId, DateTime date, CancellationToken ct = default);
        Task<bool> HasOverlappingAsync(Guid screenId, DateTime showDate, TimeOnly startTime, TimeOnly endTime, int cleaningOffsetMinutes = 0, CancellationToken ct = default);
        Task<bool> HasOverlappingExcludingAsync(Guid screenId, DateTime showDate, TimeOnly startTime, TimeOnly endTime, Guid excludeId, CancellationToken ct = default);
        Task AddAsync(Showtime showtime, CancellationToken ct = default);
        void Update(Showtime showtime);
        void Delete(Showtime showtime);
        
        // Metadata lookups for scheduling
        Task<List<Domain.Entities.SharedAggregates.SeatType>> GetSeatTypesAsync(CancellationToken ct = default);
        Task<Domain.Entities.SharedAggregates.PricingTier?> GetPricingTierAsync(Guid id, CancellationToken ct = default);
    }
}
