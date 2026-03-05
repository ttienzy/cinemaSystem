using Domain.Entities.StaffAggregate;

namespace Application.Common.Interfaces.Persistence
{
    /// <summary>
    /// Repository quản lý Ca làm (Shift) — CRUD ca làm tại rạp.
    /// Mỗi rạp có thể có nhiều ca: Sáng, Chiều, Tối.
    /// </summary>
    public interface IShiftRepository
    {
        Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Shift>> GetByCinemaAsync(Guid cinemaId, CancellationToken ct = default);
        Task AddAsync(Shift shift, CancellationToken ct = default);
        void Update(Shift shift);
        void Delete(Shift shift);
        IQueryable<Shift> GetQueryable();
    }
}
