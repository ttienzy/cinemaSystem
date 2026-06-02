using Cinema.API.Entities;

namespace Cinema.API.Repositories;

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


