using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Schedules.Commands.BulkCreateSchedule
{
    /// <summary>
    /// Bulk scheduling — Manager assigns shifts for the whole week for multiple staff.
    /// Validate each entry, skip if schedule conflict exists.
    /// </summary>
    public record BulkCreateScheduleCommand(BulkScheduleRequest Request) : IRequest<BulkScheduleResult>;

    public record BulkScheduleResult(int Created, int Skipped, List<string> Errors);

    public class BulkCreateScheduleHandler(
        IWorkScheduleRepository scheduleRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<BulkCreateScheduleCommand, BulkScheduleResult>
    {
        public async Task<BulkScheduleResult> Handle(BulkCreateScheduleCommand request, CancellationToken ct)
        {
            var created = 0;
            var skipped = 0;
            var errors = new List<string>();
            var toAdd = new List<WorkSchedule>();

            foreach (var entry in request.Request.Schedules)
            {
                // Check for conflicts for each entry
                var hasConflict = await scheduleRepo.HasConflictAsync(
                    entry.StaffId, entry.WorkDate, null, ct);

                if (hasConflict)
                {
                    skipped++;
                    errors.Add($"Staff {entry.StaffId} already scheduled for {entry.WorkDate:dd/MM/yyyy}.");
                    continue;
                }

                toAdd.Add(new WorkSchedule(entry.StaffId, entry.ShiftId, entry.WorkDate));
                created++;
            }

            if (toAdd.Any())
            {
                await scheduleRepo.AddRangeAsync(toAdd, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            return new BulkScheduleResult(created, skipped, errors);
        }
    }
}
