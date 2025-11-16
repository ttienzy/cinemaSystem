using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.InventoryDtos
{
    public class RevenueMonthlyReportRequestDto
    {
        public Guid CinemaId { get; set; }
        public DateTime StartDate { get; set; }  // Thường là đầu năm hoặc tháng
        public DateTime EndDate { get; set; }    // Thường là cuối năm hoặc tháng
    }
}
