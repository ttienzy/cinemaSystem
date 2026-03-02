using Domain.Entities.InventoryAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IInventoryRepository
    {
        Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<InventoryItem>> GetByCinemaAsync(Guid cinemaId, bool onlyAvailable = false, CancellationToken ct = default);
        Task<List<InventoryItem>> GetLowStockAsync(Guid cinemaId, CancellationToken ct = default);
        Task AddAsync(InventoryItem item, CancellationToken ct = default);
        void Update(InventoryItem item);
        IQueryable<InventoryItem> GetQueryable();
    }
}
