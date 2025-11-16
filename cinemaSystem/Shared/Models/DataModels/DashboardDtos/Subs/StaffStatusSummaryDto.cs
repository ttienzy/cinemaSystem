using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos.Subs
{
    public class StaffStatusSummaryDto
    {
        public int TotalStaffWorkingToday { get; set; }
        public int CheckedIn { get; set; }
        public int Late { get; set; }
    }

}
