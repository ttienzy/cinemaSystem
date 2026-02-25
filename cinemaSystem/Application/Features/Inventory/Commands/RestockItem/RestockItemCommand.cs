using MediatR;

namespace Application.Features.Inventory.Commands.RestockItem
{
    public record RestockItemCommand(
        Guid InventoryItemId,
        int Quantity,
        string? Note) : IRequest<RestockResult>;

    public record RestockResult(
        Guid ItemId,
        string ItemName,
        int NewStock,
        DateTime RestockedAt);
}
