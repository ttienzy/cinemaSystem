using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ClassificationDtos
{
    public class TimeSlotRequest
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DayType { get;  set; }
        public bool IsActive { get; set; }
        
    }
}
