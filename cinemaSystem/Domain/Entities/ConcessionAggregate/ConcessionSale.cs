using Domain.Common;
using Domain.Entities.BookingAggregate;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.ConcessionAggregate
{
    public class ConcessionSale : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public Guid? BookingId { get; private set; }
        public Guid StaffId { get; private set; }
        public DateTime SaleDate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string PaymentMethod { get; private set; }


        private readonly List<ConcessionSaleItem> _items = new();
        public IReadOnlyCollection<ConcessionSaleItem> Items => _items.AsReadOnly();

    }
}
