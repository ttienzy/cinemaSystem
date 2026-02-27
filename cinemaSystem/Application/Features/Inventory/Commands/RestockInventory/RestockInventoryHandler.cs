using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Inventory.Commands.RestockInventory
{
    public record RestockInventoryCommand(Guid InventoryItemId, int Quantity, string? Note) : IRequest<Unit>;

    public class RestockInventoryHandler(
        IInventoryRepository inventoryRepo,
        IUnitOfWork uow) : IRequestHandler<RestockInventoryCommand, Unit>
    {
        public async Task<Unit> Handle(RestockInventoryCommand request, CancellationToken ct)
        {
            var item = await inventoryRepo.GetByIdAsync(request.InventoryItemId, ct)
                ?? throw new NotFoundException(nameof(InventoryItem), request.InventoryItemId);

            item.Restock(request.Quantity, request.Note);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
