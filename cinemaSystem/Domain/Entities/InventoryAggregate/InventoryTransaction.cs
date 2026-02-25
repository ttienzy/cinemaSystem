using Domain.Common;

namespace Domain.Entities.InventoryAggregate
{
    /// <summary>
    /// Audit trail entity for inventory stock movements (restock/deduction).
    /// </summary>
    public class InventoryTransaction : BaseEntity
    {
        public Guid InventoryItemId { get; private set; }
        public TransactionType Type { get; private set; }
        public int Quantity { get; private set; }
        public Guid? ConcessionSaleId { get; private set; }
        public string? Note { get; private set; }
        public DateTime TransactionDate { get; private set; }

        // EF Core constructor
        private InventoryTransaction() { }

        public static InventoryTransaction CreateRestock(Guid itemId, int quantity, string? note = null)
        {
            return new InventoryTransaction
            {
                InventoryItemId = itemId,
                Type = TransactionType.Restock,
                Quantity = quantity,
                Note = note ?? "Manual restock",
                TransactionDate = DateTime.UtcNow
            };
        }

        public static InventoryTransaction CreateDeduction(
            Guid itemId, int quantity, Guid? concessionSaleId = null, string? note = null)
        {
            return new InventoryTransaction
            {
                InventoryItemId = itemId,
                Type = TransactionType.Deduction,
                Quantity = quantity,
                ConcessionSaleId = concessionSaleId,
                Note = note ?? "Sale deduction",
                TransactionDate = DateTime.UtcNow
            };
        }
    }

    public enum TransactionType
    {
        Restock = 1,
        Deduction = 2,
        Adjustment = 3
    }
}
