using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class StaffWithShiftsSpecification : Specification<Staff, StaffInfoResponse>
    {
        public StaffWithShiftsSpecification(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(s => s.CinemaId == cinemaId && s.Status == Domain.Entities.StaffAggregate.Enum.StaffStatus.Active)
                .Include(s => s.Shifts)
                 .Select(s => new StaffInfoResponse
                 {
                     FullName = s.FullName,
                     Position = s.Position,
                     Email = s.Email,
                     Phone = s.Phone,
                     Department = s.Department,
                     Shifts = s.Shifts.Where(ss => ss.ShiftDate.Date == DateTime.UtcNow.Date).Select(shift => new ShiftInfoResponse
                     {
                         StartTime = shift.StartTime,
                         EndTime = shift.EndTime,
                         ShiftDate = shift.ShiftDate
                     }).ToList()
                 });
        }
    }
}
