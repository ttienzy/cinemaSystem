using Domain.Entities.SharedAggregates;

namespace Application.Common.Interfaces.Persistence
{
    public interface IGenreRepository
    {
        Task<Genre?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Genre>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Genre genre, CancellationToken ct = default);
        void Update(Genre genre);
        void Delete(Genre genre);
    }
}
