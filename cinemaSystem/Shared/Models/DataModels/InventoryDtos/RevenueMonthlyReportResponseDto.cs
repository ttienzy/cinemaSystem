using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class RevenueMonthlyReportResponseDto
    {
        public string Month { get; set; }
        public decimal TicketRevenue { get; set; }
        public int TicketCount { get; set; }
        public decimal AverageTicketPrice { get; set; }

        public decimal ConcessionRevenue { get; set; }
        public int ConcessionCount { get; set; }
        public decimal AverageConcessionPrice { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal ConcessionRatioPercent { get; set; } 
    }

}
