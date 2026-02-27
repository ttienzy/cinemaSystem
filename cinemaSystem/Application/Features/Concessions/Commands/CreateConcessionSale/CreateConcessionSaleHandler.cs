using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.InventoryAggregate;
using MediatR;

namespace Application.Features.Concessions.Commands.CreateConcessionSale
{
    public record CreateConcessionSaleCommand(
        Guid CinemaId,
        Guid StaffId,
        string PaymentMethod,
        List<SaleItemRequest> Items,
        Guid? BookingId = null) : IRequest<Guid>;

    public record SaleItemRequest(Guid InventoryItemId, int Quantity);

    public class CreateConcessionSaleHandler(
        IConcessionSaleRepository saleRepo,
        IInventoryRepository inventoryRepo,
        IUnitOfWork uow) : IRequestHandler<CreateConcessionSaleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateConcessionSaleCommand request, CancellationToken ct)
        {
            // 1. Create the sale aggregate
            var sale = ConcessionSale.Create(request.CinemaId, request.StaffId, request.PaymentMethod, request.BookingId);

            foreach (var itemReq in request.Items)
            {
                // 2. Fetch inventory item to get latest price and check stock
                var inventoryItem = await inventoryRepo.GetByIdAsync(itemReq.InventoryItemId, ct)
                    ?? throw new NotFoundException(nameof(InventoryItem), itemReq.InventoryItemId);

                // 3. Create sale item
                var saleItem = new ConcessionSaleItem(inventoryItem.Id, itemReq.Quantity, inventoryItem.UnitPrice);
                sale.AddItem(saleItem);

                // 4. Deduct inventory (Domain logic handles validation and audit trail)
                inventoryItem.Deduct(itemReq.Quantity, sale.Id, $"Concession sale: {sale.Id.ToString()[..8]}");
                
                inventoryRepo.Update(inventoryItem);
            }

            // 5. Persist sale
            await saleRepo.AddAsync(sale, ct);
            await uow.SaveChangesAsync(ct);

            return sale.Id;
        }
    }
}
