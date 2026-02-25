using Application.Common.Interfaces.Persistence;
using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Movie aggregate repository — implements new CQRS interface.
    /// </summary>
    public class MovieRepository(BookingContext context) : IMovieRepository
    {
        public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Movies.FindAsync([id], ct);

        public async Task<Movie?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
            => await context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.CastCrew)
                .Include(m => m.Certifications)
                .Include(m => m.Copyrights)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<(List<Movie> Items, int Total)> GetPagedAsync(
            string? search, Guid? genreId, string? status,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query = context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(m =>
                    m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<MovieStatus>(status, true, out var movieStatus))
                query = query.Where(m => m.Status == movieStatus);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(m => m.ReleaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<List<Movie>> GetNowShowingAsync(DateTime date, CancellationToken ct = default)
            => await context.Movies
                .Where(m => m.Status == MovieStatus.Showing
                    && m.ReleaseDate <= date)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync(ct);

        public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
            => await context.Movies
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync(ct);

        public async Task AddAsync(Movie movie, CancellationToken ct = default)
            => await context.Movies.AddAsync(movie, ct);

        public void Update(Movie movie)
            => context.Movies.Update(movie);

        public void Delete(Movie movie)
            => context.Movies.Remove(movie);
    }
}
