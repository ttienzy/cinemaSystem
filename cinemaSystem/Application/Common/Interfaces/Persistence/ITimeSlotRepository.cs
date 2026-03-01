using Domain.Entities.SharedAggregates;

namespace Application.Common.Interfaces.Persistence
{
    public interface ITimeSlotRepository
    {
        Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<TimeSlot>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(TimeSlot timeSlot, CancellationToken ct = default);
        void Update(TimeSlot timeSlot);
        void Delete(TimeSlot timeSlot);
    }
}
