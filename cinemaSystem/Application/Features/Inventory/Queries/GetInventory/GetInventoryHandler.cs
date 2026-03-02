using Application.Common.Interfaces.Persistence;
using Domain.Entities.InventoryAggregate;
using MediatR;
using Shared.Common.Paging;

namespace Application.Features.Inventory.Queries.GetInventory
{
    public record GetInventoryQuery(Guid CinemaId, bool OnlyLowStock = false, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<InventoryDto>>;

    public record InventoryDto(
        Guid Id,
        string ItemName,
        string? Category,
        int CurrentStock,
        int MinimumStock,
        decimal UnitPrice,
        bool IsAvailable,
        DateTime? LastRestocked);

    public class GetInventoryHandler(IInventoryRepository inventoryRepo) : IRequestHandler<GetInventoryQuery, PaginatedList<InventoryDto>>
    {
        public async Task<PaginatedList<InventoryDto>> Handle(GetInventoryQuery request, CancellationToken ct)
        {
            var query = inventoryRepo.GetQueryable()
                .Where(i => i.CinemaId == request.CinemaId);

            if (request.OnlyLowStock)
            {
                query = query.Where(i => i.CurrentStock <= i.MinimumStock);
            }

            var selectQuery = query.Select(i => new InventoryDto(
                i.Id,
                i.ItemName,
                i.ItemCategory,
                i.CurrentStock,
                i.MinimumStock,
                i.UnitPrice,
                i.IsAvailable,
                i.LastRestocked));

            return await PaginatedList<InventoryDto>.CreateAsync(selectQuery, request.PageNumber, request.PageSize);
        }
    }
}
