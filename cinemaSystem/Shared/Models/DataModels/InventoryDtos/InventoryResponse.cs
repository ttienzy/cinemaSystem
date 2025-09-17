using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class InventoryResponse
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string Image { get; set; }
        public int CurrentStock { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
