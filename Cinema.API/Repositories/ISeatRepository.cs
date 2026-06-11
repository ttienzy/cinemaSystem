using Cinema.API.Entities;

namespace Cinema.API.Repositories;

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
