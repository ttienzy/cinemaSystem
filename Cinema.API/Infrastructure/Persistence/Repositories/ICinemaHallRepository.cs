using Cinema.API.Domain.Entities;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public interface ICinemaHallRepository
{
    Task<List<CinemaHall>> GetByCinemaIdAsync(Guid cinemaId);
    Task<CinemaHall?> GetByIdAsync(Guid id);
    Task<List<CinemaHall>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<List<Seat>> GetSeatsByHallIdAsync(Guid hallId);

    // CRUD operations
    Task AddAsync(CinemaHall hall);
    void Update(CinemaHall hall);
    void Delete(CinemaHall hall);
    Task SaveChangesAsync();
}


