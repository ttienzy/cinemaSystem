using Domain.Common;
using Domain.Entities.CinemaAggreagte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.StaffAggregate
{
    public class Shift : BaseEntity
    {
        public Guid StaffId { get; private set; }
        public Guid CinemaId { get; private set; } 
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public DateTime ShiftDate { get; private set; }

    }
}
