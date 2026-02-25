using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Inventory.Commands.RestockItem
{
    public class RestockItemHandler(
        IInventoryRepository inventoryRepo,
        IUnitOfWork uow) : IRequestHandler<RestockItemCommand, RestockResult>
    {
        public async Task<RestockResult> Handle(RestockItemCommand cmd, CancellationToken ct)
        {
            var item = await inventoryRepo.GetByIdAsync(cmd.InventoryItemId, ct)
                ?? throw new NotFoundException("InventoryItem", cmd.InventoryItemId);

            item.Restock(cmd.Quantity, cmd.Note);

            await uow.SaveChangesAsync(ct);

            return new RestockResult(
                item.Id, item.ItemName,
                item.CurrentStock, item.LastRestocked!.Value);
        }
    }
}
