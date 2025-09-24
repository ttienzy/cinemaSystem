using Domain.Common;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.StaffAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.StaffAggregate
{
    public class Staff : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; } 
        public string? FullName { get; private set; }
        public string? Position { get; private set; }
        public string? Department { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? Address { get; private set; }
        public DateTime HireDate { get; private set; }
        public decimal Salary { get; private set; }
        public StaffStatus Status { get; private set; } // e.g., active, on_leave, terminated

        private readonly List<Shift> _shifts = new();
        public IReadOnlyCollection<Shift> Shifts => _shifts.AsReadOnly();
        public Staff()
        {
        }
        public Staff(Guid cinemaId, string? fullName, string? position, string? department, string? phone, string? email, string? address, DateTime hireDate, decimal salary)
        {
            CinemaId = cinemaId;
            FullName = fullName;
            Position = position;
            Department = department;
            Phone = phone;
            Email = email;
            Address = address;
            HireDate = hireDate;
            Salary = salary;
            Status = StaffStatus.Active; // Default status
        }
        public void MaskAsOnLeave()
        {
            Status = StaffStatus.OnLeave;
        }

    }
}
