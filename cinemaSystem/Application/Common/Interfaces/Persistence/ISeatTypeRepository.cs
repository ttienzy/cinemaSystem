using Domain.Entities.SharedAggregates;

namespace Application.Common.Interfaces.Persistence
{
    public interface ISeatTypeRepository
    {
        Task<SeatType?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<SeatType>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(SeatType seatType, CancellationToken ct = default);
        void Update(SeatType seatType);
        void Delete(SeatType seatType);
    }
}
