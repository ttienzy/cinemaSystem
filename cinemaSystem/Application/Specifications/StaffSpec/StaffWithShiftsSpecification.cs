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
        public StaffWithShiftsSpecification(Guid cinemaId, DateTime ShiftDate)
        {
            Query.AsNoTracking()
                .Where(s => s.CinemaId == cinemaId && s.Status == Domain.Entities.StaffAggregate.Enum.StaffStatus.Active)
                .Include(ws => ws.WorkSchedules)
                .ThenInclude(ss => ss.Shift)
                 .Select(s => new StaffInfoResponse
                 {
                     Id = s.Id,
                     FullName = s.FullName,
                     Position = s.Position,
                     Email = s.Email,
                     Phone = s.Phone,
                     Department = s.Department,
                     Shifts = s.WorkSchedules.Where(ws => ws.WorkDate.Date == ShiftDate.Date).Select(shift => new ShiftInfoResponse
                     {
                         ShiftId = shift.Shift.Id,
                         StartTime = shift.Shift.DefaultStartTime,
                         EndTime = shift.Shift.DefaultEndTime,
                         ShiftDate = shift.ActualCheckInTime ?? shift.WorkDate,
                     }).ToList()
                 });
        }
    }
}
