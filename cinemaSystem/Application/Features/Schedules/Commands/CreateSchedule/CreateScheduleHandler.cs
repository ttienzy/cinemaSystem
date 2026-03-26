using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Schedules.Commands.CreateSchedule
{
    /// <summary>
    /// Assign a specific shift to an employee.
    /// Validate: no duplicate schedule on the same day.
    /// </summary>
    public record CreateScheduleCommand(ScheduleCreateRequest Request) : IRequest<Guid>;

    public class CreateScheduleHandler(
        IWorkScheduleRepository scheduleRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<CreateScheduleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken ct)
        {
            // Check for conflicts — an employee cannot work two shifts on the same day.
            var hasConflict = await scheduleRepo.HasConflictAsync(
                request.Request.StaffId, request.Request.WorkDate, null, ct);

            if (hasConflict)
                throw new InvalidOperationException(
                    $"Employee already scheduled for {request.Request.WorkDate:dd/MM/yyyy}.");

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
