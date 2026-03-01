using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class PricingTierRepository(BookingContext context) : IPricingTierRepository
    {
        public async Task<PricingTier?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Set<PricingTier>().FindAsync([id], ct);

        public async Task<List<PricingTier>> GetAllAsync(CancellationToken ct = default)
            => await context.Set<PricingTier>()
                .OrderBy(p => p.TierName)
                .ToListAsync(ct);

        public async Task AddAsync(PricingTier pricingTier, CancellationToken ct = default)
            => await context.Set<PricingTier>().AddAsync(pricingTier, ct);

        public void Update(PricingTier pricingTier)
            => context.Set<PricingTier>().Update(pricingTier);

        public void Delete(PricingTier pricingTier)
            => context.Set<PricingTier>().Remove(pricingTier);
    }
}
