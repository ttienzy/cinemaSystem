using Cinema.API.Domain.Entities;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public interface ISeatRepository
{
    Task<List<Seat>> GetByHallIdAsync(Guid hallId);
    Task<Seat?> GetByIdAsync(Guid id);
    Task<List<Seat>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Seat?> GetByRowAndNumberAsync(Guid hallId, string row, int number);

    Task AddAsync(Seat seat);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    void Update(Seat seat);
    void Delete(Seat seat);
    void DeleteRange(IEnumerable<Seat> seats);
    Task SaveChangesAsync();
}
