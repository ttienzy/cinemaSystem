using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.DashboardDtos
{
    public class CinemaDashboardRequest
    {
        public Guid CinemaId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }  // Optional
    }

}
