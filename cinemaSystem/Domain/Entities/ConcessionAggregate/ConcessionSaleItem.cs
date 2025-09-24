using Domain.Common;
using Domain.Entities.InventoryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.ConcessionAggregate
{
    public class ConcessionSaleItem : BaseEntity
    {
        public Guid ConcessionSaleId { get; private set; }
        public Guid InventoryId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public ConcessionSaleItem()
        {
        }
        public ConcessionSaleItem( Guid inventoryId, int quantity, decimal unitPrice)
        {
            InventoryId = inventoryId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

    }
}
