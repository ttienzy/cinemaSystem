using Domain.Entities.MovieAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IMovieRepository
    {
        Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Movie?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<(List<Movie> Items, int Total)> GetPagedAsync(
            string? search, Guid? genreId, Guid? cinemaId, string? status, int page, int pageSize, CancellationToken ct = default);
        Task<List<Movie>> GetNowShowingAsync(DateTime date, CancellationToken ct = default);
        Task AddAsync(Movie movie, CancellationToken ct = default);
        void Update(Movie movie);
        void Delete(Movie movie);
    }
}
