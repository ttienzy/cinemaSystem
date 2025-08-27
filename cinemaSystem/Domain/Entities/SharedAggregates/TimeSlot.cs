using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SharedAggregates
{
    public class TimeSlot : BaseEntity, IAggregateRoot
    {
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public string DayType { get; private set; } // e.g., weekday, weekend, holiday
        public bool IsActive { get; private set; }
    }
}
