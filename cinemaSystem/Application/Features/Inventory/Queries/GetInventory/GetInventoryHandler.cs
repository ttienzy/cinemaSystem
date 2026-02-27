using Application.Common.Interfaces.Persistence;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventory.Queries.GetInventory
{
    public record GetInventoryQuery(Guid CinemaId, bool OnlyLowStock = false) : IRequest<List<InventoryDto>>;

    public record InventoryDto(
        Guid Id,
        string ItemName,
        string? Category,
        int CurrentStock,
        int MinimumStock,
        decimal UnitPrice,
        bool IsAvailable,
        DateTime? LastRestocked);

    public class GetInventoryHandler(IInventoryRepository inventoryRepo) : IRequestHandler<GetInventoryQuery, List<InventoryDto>>
    {
        public async Task<List<InventoryDto>> Handle(GetInventoryQuery request, CancellationToken ct)
        {
            List<InventoryItem> items;
            
            if (request.OnlyLowStock)
            {
                items = await inventoryRepo.GetLowStockAsync(request.CinemaId, ct);
            }
            else
            {
                items = await inventoryRepo.GetByCinemaAsync(request.CinemaId, false, ct);
            }

            return items.Select(i => new InventoryDto(
                i.Id,
                i.ItemName,
                i.ItemCategory,
                i.CurrentStock,
                i.MinimumStock,
                i.UnitPrice,
                i.IsAvailable,
                i.LastRestocked)).ToList();
        }
    }
}
