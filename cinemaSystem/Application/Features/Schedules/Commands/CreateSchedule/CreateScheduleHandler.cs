using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Schedules.Commands.CreateSchedule
{
    /// <summary>
    /// Xếp lịch cho 1 nhân viên vào 1 ca làm cụ thể.
    /// Validate: không trùng lịch cùng ngày.
    /// </summary>
    public record CreateScheduleCommand(ScheduleCreateRequest Request) : IRequest<Guid>;

    public class CreateScheduleHandler(
        IWorkScheduleRepository scheduleRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<CreateScheduleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken ct)
        {
            // Kiểm tra xung đột — 1 nhân viên không thể làm 2 ca cùng ngày
            var hasConflict = await scheduleRepo.HasConflictAsync(
                request.Request.StaffId, request.Request.WorkDate, null, ct);

            if (hasConflict)
                throw new InvalidOperationException(
                    $"Nhân viên đã có lịch làm ngày {request.Request.WorkDate:dd/MM/yyyy}.");

            var schedule = new WorkSchedule(
                request.Request.StaffId,
                request.Request.ShiftId,
                request.Request.WorkDate);

            await scheduleRepo.AddAsync(schedule, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return schedule.Id;
        }
    }
}
