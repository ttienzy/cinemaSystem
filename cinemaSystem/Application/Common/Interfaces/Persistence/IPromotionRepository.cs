using Domain.Entities.PromotionAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IPromotionRepository
    {
        Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Promotion?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<List<Promotion>> GetActiveAsync(CancellationToken ct = default);
        Task AddAsync(Promotion promotion, CancellationToken ct = default);
        void Update(Promotion promotion);
    }
}
