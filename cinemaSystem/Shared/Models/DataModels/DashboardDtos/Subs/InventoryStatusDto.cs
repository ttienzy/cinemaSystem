using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos.Subs
{
    public class InventoryStatusDto
    {
        public string ItemName { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string StockStatus { get; set; }
    }

}
