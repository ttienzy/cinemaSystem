using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Schedules.Queries.GetWeeklySchedule
{
    /// <summary>
    /// Get weekly schedule — displayed in table: staff x day x shift.
    /// </summary>
    public record GetWeeklyScheduleQuery(Guid CinemaId, DateTime WeekOf) : IRequest<List<WorkScheduleDto>>;

    public class GetWeeklyScheduleHandler(IWorkScheduleRepository scheduleRepo)
        : IRequestHandler<GetWeeklyScheduleQuery, List<WorkScheduleDto>>
    {
        public async Task<List<WorkScheduleDto>> Handle(GetWeeklyScheduleQuery request, CancellationToken ct)
        {
            // Calculate the start of the week (Monday)
            var weekStart = request.WeekOf.Date;
            var dayOfWeek = (int)weekStart.DayOfWeek;
            weekStart = weekStart.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));

            var schedules = await scheduleRepo.GetByCinemaAndWeekAsync(request.CinemaId, weekStart, ct);

            return schedules.Select(ws => new WorkScheduleDto
            {
                Id = ws.Id,
                StaffId = ws.StaffId,
                StaffName = ws.Staff?.FullName,
                ShiftId = ws.ShiftId,
                ShiftName = ws.Shift?.Name,
                ShiftTime = $"{ws.Shift?.DefaultStartTime:hh\\:mm} - {ws.Shift?.DefaultEndTime:hh\\:mm}",
                WorkDate = ws.WorkDate,
                ActualCheckInTime = ws.ActualCheckInTime
            }).ToList();
        }
    }
}
