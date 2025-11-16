using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos.Subs
{
    public class ShowtimeOccupancyDto
    {
        public string Movie { get; set; }
        public DateTime ActualStartTime { get; set; }
        public string ScreenName { get; set; }
        public int TotalSeats { get; set; }
        public int SoldSeats { get; set; }
        public decimal OccupancyRate { get; set; }
    }

}
