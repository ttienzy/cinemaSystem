using Domain.Entities.StaffAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.StaffDtos
{
    public class GetStaffToCinemaResponse
    {
        public Guid Id { get;  set; }
        public string? FullName { get;  set; }
        public string? Position { get;  set; }
        public string? Department { get;  set; }
        public string? Phone { get;  set; }
        public string? Email { get;  set; }
        public string? Address { get;  set; }
        public DateTime HireDate { get;  set; }
        public decimal Salary { get;  set; }
        public StaffStatus Status { get;  set; }
    }
}
