using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class GenreRepository(BookingContext context) : IGenreRepository
    {
        public async Task<Genre?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<Genre>().FindAsync([id], ct);

        public async Task<List<Genre>> GetAllAsync(CancellationToken ct = default)
            => await context.Set<Genre>()
                .OrderBy(g => g.GenreName)
                .ToListAsync(ct);

        public async Task AddAsync(Genre genre, CancellationToken ct = default)
            => await context.Set<Genre>().AddAsync(genre, ct);

        public void Update(Genre genre)
            => context.Set<Genre>().Update(genre);

        public void Delete(Genre genre)
            => context.Set<Genre>().Remove(genre);
    }
}
