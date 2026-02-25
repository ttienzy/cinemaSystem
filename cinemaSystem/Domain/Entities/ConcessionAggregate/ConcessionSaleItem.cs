using Domain.Common;

namespace Domain.Entities.ConcessionAggregate
{
    /// <summary>
    /// ConcessionSaleItem — enhanced with LineTotal computed property.
    /// </summary>
    public class ConcessionSaleItem : BaseEntity
    {
        public Guid ConcessionSaleId { get; private set; }
        public Guid InventoryItemId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public decimal LineTotal => Quantity * UnitPrice;

        // EF Core constructor
        private ConcessionSaleItem() { }

        public ConcessionSaleItem(Guid inventoryItemId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be at least 1.");
            if (unitPrice < 0)
                throw new DomainException("Unit price cannot be negative.");

            InventoryItemId = inventoryItemId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new DomainException("Quantity must be at least 1.");
            Quantity = newQuantity;
        }
    }
}
