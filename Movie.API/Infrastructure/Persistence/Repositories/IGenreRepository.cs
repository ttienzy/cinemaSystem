using Movie.API.Domain.Entities;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync();
    Task<Genre?> GetByIdAsync(Guid id);
    Task<Genre> CreateAsync(Genre genre);
    Task<Genre?> UpdateAsync(Guid id, Genre genre);
    Task<bool> DeleteAsync(Guid id);
}



