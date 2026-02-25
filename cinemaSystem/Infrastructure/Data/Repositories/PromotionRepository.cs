using Application.Common.Interfaces.Persistence;
using Domain.Entities.PromotionAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    /// <summary>
    /// Promotion aggregate repository — new aggregate added during DDD refactoring.
    /// </summary>
    public class PromotionRepository(BookingContext context) : IPromotionRepository
    {
        public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<Promotion>().FindAsync([id], ct);

        public async Task<Promotion?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await context.Set<Promotion>()
                .FirstOrDefaultAsync(p => p.Code == code, ct);

        public async Task<List<Promotion>> GetActiveAsync(CancellationToken ct = default)
            => await context.Set<Promotion>()
                .Where(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .ToListAsync(ct);

        public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
            => await context.Set<Promotion>().AddAsync(promotion, ct);

        public void Update(Promotion promotion)
            => context.Set<Promotion>().Update(promotion);
    }
}
