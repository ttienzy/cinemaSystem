using Domain.Entities.CinemaAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface ICinemaRepository
    {
        Task<Cinema?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Cinema?> GetByIdWithScreensAsync(Guid id, CancellationToken ct = default);
        Task<Cinema?> GetByIdWithScreensAndSeatsAsync(Guid id, CancellationToken ct = default);
        Task<Screen?> GetScreenByIdAsync(Guid screenId, CancellationToken ct = default);
        Task<Screen?> GetScreenWithSeatsAsync(Guid screenId, CancellationToken ct = default);
        Task<List<Cinema>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Cinema cinema, CancellationToken ct = default);
        void Update(Cinema cinema);
    }
}
