using Application.Common.Interfaces.Persistence;
using Domain.Entities.InventoryAggregate;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class InventoryRepository(BookingContext context) : IInventoryRepository
    {
        public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.InventoryItems.FindAsync([id], ct);

        public async Task<List<InventoryItem>> GetByCinemaAsync(
            Guid cinemaId, bool onlyAvailable = false, CancellationToken ct = default)
        {
            var query = context.InventoryItems.Where(i => i.CinemaId == cinemaId);
            if (onlyAvailable)
                query = query.Where(i => i.CurrentStock > 0);
            return await query.ToListAsync(ct);
        }

        public async Task<List<InventoryItem>> GetLowStockAsync(Guid cinemaId, CancellationToken ct = default)
            => await context.InventoryItems
                .Where(i => i.CinemaId == cinemaId && i.CurrentStock <= i.MinimumStock)
                .ToListAsync(ct);

        public async Task AddAsync(InventoryItem item, CancellationToken ct = default)
            => await context.InventoryItems.AddAsync(item, ct);

        public void Update(InventoryItem item)
            => context.InventoryItems.Update(item);

        public IQueryable<InventoryItem> GetQueryable()
            => context.InventoryItems;
    }
}
