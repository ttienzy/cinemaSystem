using Domain.Common;
using Domain.Entities.CinemaAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.StaffAggregate
{
    public class Shift : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public string? Name { get; private set; }
        public TimeSpan DefaultStartTime { get; private set; }
        public TimeSpan DefaultEndTime { get; private set; }
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }


        public Shift() { }
        public Shift(Guid cinemaId, string? name, TimeSpan defaultStartTime, TimeSpan defaultEndTime)
        {
            CinemaId = cinemaId;
            Name = name;
            DefaultStartTime = defaultStartTime;
            DefaultEndTime = defaultEndTime;
        }
        public void UpdateShift(Guid cinemaId, string? name, TimeSpan defaultStartTime, TimeSpan defaultEndTime)
        {
            CinemaId = cinemaId;
            Name = name;
            DefaultStartTime = defaultStartTime;
            DefaultEndTime = defaultEndTime;
        }
    }
}
