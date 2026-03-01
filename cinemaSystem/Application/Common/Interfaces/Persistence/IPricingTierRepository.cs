using Domain.Entities.SharedAggregates;

namespace Application.Common.Interfaces.Persistence
{
    public interface IPricingTierRepository
    {
        Task<PricingTier?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<PricingTier>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(PricingTier pricingTier, CancellationToken ct = default);
        void Update(PricingTier pricingTier);
        void Delete(PricingTier pricingTier);
    }
}
