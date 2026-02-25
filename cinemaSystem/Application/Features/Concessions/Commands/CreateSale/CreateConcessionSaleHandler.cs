using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.ConcessionAggregate;
using MediatR;

namespace Application.Features.Concessions.Commands.CreateSale
{
    public class CreateConcessionSaleHandler(
        IConcessionSaleRepository saleRepo,
        IInventoryRepository inventoryRepo,
        IPromotionRepository promotionRepo,
        IUnitOfWork uow) : IRequestHandler<CreateConcessionSaleCommand, CreateConcessionSaleResult>
    {
        public async Task<CreateConcessionSaleResult> Handle(
            CreateConcessionSaleCommand cmd, CancellationToken ct)
        {
            // 1. Create sale
            var sale = ConcessionSale.Create(
                cmd.CinemaId, cmd.StaffId, cmd.PaymentMethod, cmd.BookingId);

            // 2. Add items and deduct inventory
            foreach (var item in cmd.Items)
            {
                var inventoryItem = await inventoryRepo.GetByIdAsync(item.InventoryItemId, ct)
                    ?? throw new NotFoundException("InventoryItem", item.InventoryItemId);

                if (!inventoryItem.IsAvailable)
                    throw new ConflictException($"Item '{inventoryItem.ItemName}' is not available.");

                // Create sale item with current price
                var saleItem = new ConcessionSaleItem(
                    item.InventoryItemId, item.Quantity, inventoryItem.UnitPrice);
                sale.AddItem(saleItem);

                // Deduct stock (raises LowStockAlertEvent if below threshold)
                inventoryItem.Deduct(item.Quantity, sale.Id);
            }

            // 3. Apply promotion if provided
            if (!string.IsNullOrEmpty(cmd.PromotionCode))
            {
                var promo = await promotionRepo.GetByCodeAsync(cmd.PromotionCode, ct);
                if (promo is not null)
                {
                    var (canApply, discount, _) = promo.Evaluate(sale.SubTotal);
                    if (canApply)
                    {
                        sale.ApplyDiscount(promo.Id, discount);
                        promo.IncrementUsage();
                    }
                }
            }

            // 4. Persist
            await saleRepo.AddAsync(sale, ct);
            await uow.SaveChangesAsync(ct);

            return new CreateConcessionSaleResult(
                sale.Id, sale.SubTotal, sale.DiscountAmount, sale.TotalAmount);
        }
    }
}
