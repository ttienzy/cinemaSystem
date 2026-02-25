using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Cinema aggregate repository — implements new CQRS interface.
    /// Old reporting queries remain in Data/Services/CinemaService.
    /// </summary>
    public class CinemaRepository(BookingContext context) : ICinemaRepository
    {
        public async Task<Cinema?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Cinemas.FindAsync([id], ct);

        public async Task<Cinema?> GetByIdWithScreensAsync(Guid id, CancellationToken ct = default)
            => await context.Cinemas
                .Include(c => c.Screens)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<Cinema?> GetByIdWithScreensAndSeatsAsync(Guid id, CancellationToken ct = default)
            => await context.Cinemas
                .Include(c => c.Screens)
                    .ThenInclude(s => s.Seats)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<Screen?> GetScreenByIdAsync(Guid screenId, CancellationToken ct = default)
            => await context.Screens.FindAsync([screenId], ct);

        public async Task<Screen?> GetScreenWithSeatsAsync(Guid screenId, CancellationToken ct = default)
            => await context.Screens
                .Include(s => s.Seats)
                .FirstOrDefaultAsync(s => s.Id == screenId, ct);

        public async Task<List<Cinema>> GetAllAsync(CancellationToken ct = default)
            => await context.Cinemas
                .Include(c => c.Screens)
                .ToListAsync(ct);

        public async Task AddAsync(Cinema cinema, CancellationToken ct = default)
            => await context.Cinemas.AddAsync(cinema, ct);

        public void Update(Cinema cinema)
            => context.Cinemas.Update(cinema);
    }
}
