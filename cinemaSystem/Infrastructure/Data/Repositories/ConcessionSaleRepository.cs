using Application.Common.Interfaces.Persistence;
using Domain.Entities.ConcessionAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// ConcessionSale aggregate repository — implements new CQRS interface.
    /// </summary>
    public class ConcessionSaleRepository(BookingContext context) : IConcessionSaleRepository
    {
        public async Task<ConcessionSale?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.ConcessionSales.FindAsync([id], ct);

        public async Task<ConcessionSale?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
            => await context.ConcessionSales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<List<ConcessionSale>> GetByCinemaAndDateAsync(
            Guid cinemaId, DateTime date, CancellationToken ct = default)
            => await context.ConcessionSales
                .Where(s => s.CinemaId == cinemaId && s.SaleDate.Date == date.Date)
                .Include(s => s.Items)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync(ct);

        public async Task<(List<ConcessionSale> Items, int Total)> GetPagedAsync(
            Guid cinemaId, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query = context.ConcessionSales
                .Where(s => s.CinemaId == cinemaId);

            if (fromDate.HasValue)
                query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(s => s.SaleDate <= toDate.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(s => s.Items)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task AddAsync(ConcessionSale sale, CancellationToken ct = default)
            => await context.ConcessionSales.AddAsync(sale, ct);

        public void Update(ConcessionSale sale)
            => context.ConcessionSales.Update(sale);

        public IQueryable<ConcessionSale> GetQueryable()
            => context.ConcessionSales;
    }
}
