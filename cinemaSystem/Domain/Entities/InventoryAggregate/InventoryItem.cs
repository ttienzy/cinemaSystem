using Domain.Common;
using Domain.Events;

namespace Domain.Entities.InventoryAggregate
{
    /// <summary>
    /// InventoryItem aggregate — tracks concession stock with restock/deduct audit trail.
    /// Raises LowStockAlertEvent when stock falls at or below minimum threshold.
    /// </summary>
    public class InventoryItem : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public string ItemName { get; private set; } = string.Empty;
        public string? ItemCategory { get; private set; }
        public int CurrentStock { get; private set; }
        public int MinimumStock { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal CostPrice { get; private set; }
        public string? ImageUrl { get; private set; }

        // ── New fields ──────────────────────────────────────────────
        public bool IsAvailable { get; private set; } = true;
        public string? SupplierInfo { get; private set; }
        public DateTime? LastRestocked { get; private set; }

        private readonly List<InventoryTransaction> _transactions = [];
        public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

        // EF Core constructor
        private InventoryItem() { }

        public InventoryItem(
            Guid cinemaId, string itemName, string? itemCategory,
            int currentStock, int minimumStock,
            decimal unitPrice, decimal costPrice, string? imageUrl,
            string? supplierInfo = null)
        {
            CinemaId = cinemaId;
            ItemName = itemName;
            ItemCategory = itemCategory;
            CurrentStock = currentStock;
            MinimumStock = minimumStock;
            UnitPrice = unitPrice;
            CostPrice = costPrice;
            ImageUrl = imageUrl;
            SupplierInfo = supplierInfo;
            IsAvailable = true;
        }

        // ── Commands ─────────────────────────────────────────────────

        public void Restock(int quantity, string? note = null)
        {
            if (quantity <= 0)
                throw new DomainException("Restock quantity must be positive.");

            CurrentStock += quantity;
            LastRestocked = DateTime.UtcNow;

            _transactions.Add(InventoryTransaction.CreateRestock(Id, quantity, note));
        }

        public void Deduct(int quantity, Guid? concessionSaleId = null, string? note = null)
        {
            if (quantity <= 0)
                throw new DomainException("Deduct quantity must be positive.");
            if (CurrentStock < quantity)
                throw new DomainException($"Not enough stock for '{ItemName}'. Current: {CurrentStock}, Requested: {quantity}.");

            CurrentStock -= quantity;
            _transactions.Add(InventoryTransaction.CreateDeduction(Id, quantity, concessionSaleId, note));

            // Alert if stock is at or below minimum
            if (CurrentStock <= MinimumStock)
                Raise(new LowStockAlertEvent(Id, ItemName, CurrentStock, MinimumStock));
        }

        public void ToggleAvailability()
        {
            IsAvailable = !IsAvailable;
        }

        public void UpdateDetails(
            string itemName, string? itemCategory,
            int minimumStock, decimal unitPrice, decimal costPrice,
            string? imageUrl, string? supplierInfo)
        {
            ItemName = itemName;
            ItemCategory = itemCategory;
            MinimumStock = minimumStock;
            UnitPrice = unitPrice;
            CostPrice = costPrice;
            ImageUrl = imageUrl;
            SupplierInfo = supplierInfo;
        }

        // Legacy compatibility
        public void DecreaseStock(int quantity) => Deduct(quantity);
    }
}
