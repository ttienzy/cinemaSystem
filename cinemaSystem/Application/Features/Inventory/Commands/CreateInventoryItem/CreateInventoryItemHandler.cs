using Application.Common.Interfaces.Persistence;
using Domain.Entities.InventoryAggregate;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Inventory.Commands.CreateInventoryItem
{
    public record CreateInventoryItemRequest(
        Guid CinemaId,
        string ItemName,
        string? ItemCategory,
        int InitialStock,
        int MinimumStock,
        decimal UnitPrice,
        decimal CostPrice,
        string? ImageUrl,
        string? SupplierInfo);

    public record CreateInventoryItemCommand(CreateInventoryItemRequest Request) : IRequest<Guid>;

    public class CreateInventoryItemHandler(IInventoryRepository inventoryRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<CreateInventoryItemCommand, Guid>
    {
        public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken ct)
        {
            var item = new InventoryItem(
                request.Request.CinemaId,
                request.Request.ItemName,
                request.Request.ItemCategory,
                request.Request.InitialStock,
                request.Request.MinimumStock,
                request.Request.UnitPrice,
                request.Request.CostPrice,
                request.Request.ImageUrl,
                request.Request.SupplierInfo
            );

            await inventoryRepo.AddAsync(item, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return item.Id;
        }
    }
}
