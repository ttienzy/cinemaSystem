using Movie.API.Entities;

namespace Movie.API.Repositories;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync();
    Task<Genre?> GetByIdAsync(Guid id);
    Task<Genre> CreateAsync(Genre genre);
    Task<Genre?> UpdateAsync(Guid id, Genre genre);
    Task<bool> DeleteAsync(Guid id);
}



