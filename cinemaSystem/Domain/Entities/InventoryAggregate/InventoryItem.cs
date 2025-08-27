using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.InventoryAggregate
{
    public class InventoryItem : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; } // FK to Cinema Aggregate Root
        public string ItemName { get; private set; }
        public string? ItemCategory { get; private set; }
        public int CurrentStock { get; private set; }
        public int MinimumStock { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal CostPrice { get; private set; }
        public string SupplierInfo { get; private set; }
        public DateTime LastRestocked { get; private set; }
    }
}
