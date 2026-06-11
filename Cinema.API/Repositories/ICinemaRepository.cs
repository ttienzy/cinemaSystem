using CinemaEntity = Cinema.API.Entities.Cinema;

namespace Cinema.API.Repositories;

public interface ICinemaRepository
{
    Task<List<CinemaEntity>> GetAllAsync();
    Task<CinemaEntity?> GetByIdAsync(Guid id);
    Task<CinemaEntity> CreateAsync(CinemaEntity cinema);
    Task<CinemaEntity?> UpdateAsync(Guid id, CinemaEntity cinema);
    Task<bool> DeleteAsync(Guid id);
}



