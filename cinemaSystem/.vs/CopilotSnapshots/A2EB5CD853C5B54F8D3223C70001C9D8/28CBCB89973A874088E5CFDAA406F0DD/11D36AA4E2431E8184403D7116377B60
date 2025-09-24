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
        public string? ImageUrl { get; private set; }
        public InventoryItem() { }

        public InventoryItem(Guid cinemaId, string itemName, string? itemCategory, int currentStock, int minimumStock, decimal unitPrice, decimal costPrice, string? imageUrl)
        {
            CinemaId = cinemaId;
            ItemName = itemName;
            ItemCategory = itemCategory;
            CurrentStock = currentStock;
            MinimumStock = minimumStock;
            UnitPrice = unitPrice;
            CostPrice = costPrice;
            ImageUrl = imageUrl;
        }
        
    }
}
