using Domain.Common;

namespace Domain.Entities.ConcessionAggregate
{
    /// <summary>
    /// ConcessionSale aggregate — enhanced with SubTotal, DiscountAmount,
    /// AddItem() with auto-recalculation, and RecalculateTotal().
    /// </summary>
    public class ConcessionSale : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public Guid? BookingId { get; private set; }
        public Guid StaffId { get; private set; }
        public DateTime SaleDate { get; private set; }
        public decimal SubTotal { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string? PaymentMethod { get; private set; }
        public Guid? PromotionId { get; private set; }

        private readonly List<ConcessionSaleItem> _items = [];
        public IReadOnlyCollection<ConcessionSaleItem> Items => _items.AsReadOnly();

        // EF Core constructor
        private ConcessionSale() { }

        public static ConcessionSale Create(
            Guid cinemaId, Guid staffId, string paymentMethod,
            Guid? bookingId = null)
        {
            return new ConcessionSale
            {
                CinemaId = cinemaId,
                StaffId = staffId,
                BookingId = bookingId,
                SaleDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                SubTotal = 0,
                DiscountAmount = 0,
                TotalAmount = 0
            };
        }

        public void AddItem(ConcessionSaleItem item)
        {
            _items.Add(item);
            RecalculateTotal();
        }

        public void AddItems(List<ConcessionSaleItem> items)
        {
            _items.AddRange(items);
            RecalculateTotal();
        }

        public void RemoveItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotal();
            }
        }

        public void ApplyDiscount(Guid promotionId, decimal discountAmount)
        {
            PromotionId = promotionId;
            DiscountAmount = discountAmount;
            TotalAmount = Math.Max(0, SubTotal - DiscountAmount);
        }

        private void RecalculateTotal()
        {
            SubTotal = _items.Sum(i => i.LineTotal);
            TotalAmount = Math.Max(0, SubTotal - DiscountAmount);
        }

        // Legacy constructor
        public ConcessionSale(Guid cinemaId, Guid? bookingId, Guid staffId, decimal totalAmount, string? paymentMethod)
        {
            CinemaId = cinemaId;
            BookingId = bookingId;
            StaffId = staffId;
            SaleDate = DateTime.UtcNow;
            TotalAmount = totalAmount;
            SubTotal = totalAmount;
            PaymentMethod = paymentMethod;
        }
    }
}
