using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Schedules.Commands.BulkCreateSchedule
{
    /// <summary>
    /// Xếp lịch hàng loạt — Manager xếp cả tuần 1 lần cho nhiều nhân viên.
    /// Validate từng entry, bỏ qua entry trùng lịch.
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
                // Kiểm tra xung đột cho từng entry
                var hasConflict = await scheduleRepo.HasConflictAsync(
                    entry.StaffId, entry.WorkDate, null, ct);

                if (hasConflict)
                {
                    skipped++;
                    errors.Add($"Nhân viên {entry.StaffId} đã có lịch ngày {entry.WorkDate:dd/MM/yyyy}.");
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
