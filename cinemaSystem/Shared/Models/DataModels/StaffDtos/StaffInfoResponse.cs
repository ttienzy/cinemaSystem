using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.StaffDtos
{
    public class StaffInfoResponse
    {
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public List<ShiftInfoResponse> Shifts { get; set; } = new List<ShiftInfoResponse>();
    }
    public class ShiftInfoResponse
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ShiftDate { get; set; }
    }
}
