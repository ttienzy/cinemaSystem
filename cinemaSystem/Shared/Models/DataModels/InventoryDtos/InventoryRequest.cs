using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class InventoryRequest
    {
        public Guid CinemaId { get;  set; } 
        public string ItemName { get;  set; }
        public string? ItemCategory { get;  set; }
        public int CurrentStock { get;  set; }
        public int MinimumStock { get; set; }
        public decimal UnitPrice { get;  set; }
        public decimal CostPrice { get;  set; }
        public string? ImageUrl { get;  set; }
    }
}
