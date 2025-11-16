using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class RevenueReportResponseDto
    {
        public DateTime Date { get; set; }

        // Vé xem phim
        public decimal TicketRevenue { get; set; }
        public int TicketCount { get; set; }
        public decimal AverageTicketPrice { get; set; }

        // Đồ ăn/uống
        public decimal ConcessionRevenue { get; set; }
        public int ConcessionCount { get; set; }
        public decimal AverageConcessionPrice { get; set; }

        // Tổng cộng
        public decimal TotalRevenue { get; set; }

        // Tỷ lệ doanh thu đồ ăn/vé
        public decimal ConcessionRatioPercent { get; set; }
    }

}
