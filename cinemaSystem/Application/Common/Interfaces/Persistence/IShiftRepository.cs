using Domain.Entities.StaffAggregate;

namespace Application.Common.Interfaces.Persistence
{
    /// <summary>
    /// Shift Repository — CRUD operations for cinema shifts.
    /// Each cinema can have multiple shifts: Morning, Afternoon, Evening.
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
