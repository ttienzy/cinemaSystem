using Domain.Common;
using Domain.Entities.SharedAggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.CinemaAggreagte
{
    public class Seat : BaseEntity
    {
        public Guid ScreenId { get; private set; }
        public Guid SeatTypeId { get; private set; } 
        public string RowName { get; private set; } 
        public int Number { get; private set; } 
        public bool IsActive { get; private set; }
        public bool IsBlocked { get; private set; }

        public Seat()
        {
            Id = Guid.NewGuid();
        }
        public Seat(Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked)
        {
            Id = Guid.NewGuid();
            SeatTypeId = seatTypeId;
            RowName = rowName;
            Number = number;
            IsActive = isActive;
            IsBlocked = isBlocked;
        }
        public void UpdateDetails(Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked)
        {
            SeatTypeId = seatTypeId;
            RowName = rowName;
            Number = number;
            IsActive = isActive;
            IsBlocked = isBlocked;
        }
        public void MarkAsBlocked()
        {
            IsBlocked = true;
        }
    }
}
