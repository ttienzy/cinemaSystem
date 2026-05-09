using Movie.API.Domain.Entities;
using MovieEntity = Movie.API.Domain.Entities.Movie;

namespace Movie.API.Infrastructure.Persistence.Repositories;

public interface IMovieRepository
{
    Task<List<MovieEntity>> GetAllAsync();
    Task<MovieEntity?> GetByIdAsync(Guid id);
    Task<MovieEntity> CreateAsync(MovieEntity movie);
    Task<MovieEntity?> UpdateAsync(Guid id, MovieEntity movie, IEnumerable<Guid> genreIds);
    Task<bool> DeleteAsync(Guid id);
    Task<List<MovieEntity>> GetByGenreAsync(Guid genreId);
}




