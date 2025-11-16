using Shared.Models.DataModels.DashboardDtos.Subs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos
{
    public class CinemaDashboardResponse
    {
        public TicketReportDto TicketReport { get; set; }
        public ConcessionReportDto ConcessionReport { get; set; }
        public StaffStatusSummaryDto StaffSummary { get; set; }
        public List<ShowtimeOccupancyDto> ShowtimeStats { get; set; }
        public List<InventoryStatusDto> InventoryStatus { get; set; }
    }

}
