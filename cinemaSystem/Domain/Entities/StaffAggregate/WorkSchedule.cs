using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.StaffAggregate
{
    public class WorkSchedule : BaseEntity, IAggregateRoot
    {
        public Guid StaffId { get; private set; }
        public Guid ShiftId { get; private set; }
        public DateTime WorkDate { get; private set; }
        public DateTime? ActualCheckInTime { get; private set; }
        public virtual Staff Staff { get; set; }
        public virtual Shift Shift { get; set; }

        public WorkSchedule() { }
        public WorkSchedule(Guid staffId, Guid shiftId, DateTime actualCheckInTime)
        {
            StaffId = staffId;
            ShiftId = shiftId;
            ActualCheckInTime = actualCheckInTime;
            WorkDate = actualCheckInTime.Date;
        }
    }
}
