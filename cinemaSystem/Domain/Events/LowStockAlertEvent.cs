using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when inventory stock falls at or below the minimum threshold.</summary>
    public class LowStockAlertEvent : BaseDomainEvent
    {
        public Guid InventoryItemId { get; }
        public string ItemName { get; }
        public int CurrentStock { get; }
        public int MinimumStock { get; }

        public LowStockAlertEvent(
            Guid inventoryItemId, string itemName,
            int currentStock, int minimumStock)
        {
            InventoryItemId = inventoryItemId;
            ItemName = itemName;
            CurrentStock = currentStock;
            MinimumStock = minimumStock;
        }
    }
}
