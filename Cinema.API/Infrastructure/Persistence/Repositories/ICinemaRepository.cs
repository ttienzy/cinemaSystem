using Cinema.API.Domain.Entities;
using CinemaEntity = Cinema.API.Domain.Entities.Cinema;

namespace Cinema.API.Infrastructure.Persistence.Repositories;

public interface ICinemaRepository
{
    Task<List<CinemaEntity>> GetAllAsync();
    Task<CinemaEntity?> GetByIdAsync(Guid id);
    Task<CinemaEntity> CreateAsync(CinemaEntity cinema);
    Task<CinemaEntity?> UpdateAsync(Guid id, CinemaEntity cinema);
    Task<bool> DeleteAsync(Guid id);
}



