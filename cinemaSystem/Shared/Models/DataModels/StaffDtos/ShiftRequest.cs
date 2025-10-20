using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.StaffDtos
{
    public class ShiftRequest
    {
        public Guid CinemaId { get; set; }  
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Name { get; set; }
    }
}
