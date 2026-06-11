using MovieEntity = Movie.API.Entities.Movie;

namespace Movie.API.Repositories;

public interface IMovieRepository
{
    Task<List<MovieEntity>> GetAllAsync();
    Task<MovieEntity?> GetByIdAsync(Guid id);
    Task<MovieEntity> CreateAsync(MovieEntity movie);
    Task<MovieEntity?> UpdateAsync(Guid id, MovieEntity movie, IEnumerable<Guid> genreIds, bool updateEmbedding = false);
    Task<bool> DeleteAsync(Guid id);
    Task<List<MovieEntity>> GetByGenreAsync(Guid genreId);
}




