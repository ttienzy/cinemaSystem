using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Shifts.Commands.DeleteShift
{
    /// <summary>
    /// Delete shift — ensures no active schedules exist before deletion.
    /// </summary>
    public record DeleteShiftCommand(Guid ShiftId) : IRequest;

    public class DeleteShiftHandler(
        IShiftRepository shiftRepo,
        IWorkScheduleRepository scheduleRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteShiftCommand>
    {
        public async Task Handle(DeleteShiftCommand request, CancellationToken ct)
        {
            var shift = await shiftRepo.GetByIdAsync(request.ShiftId, ct)
                ?? throw new KeyNotFoundException($"Shift not found with ID: {request.ShiftId}");

            // Check for future schedules
            var hasActiveSchedules = scheduleRepo.GetQueryable()
                .Any(ws => ws.ShiftId == request.ShiftId && ws.WorkDate >= DateTime.UtcNow.Date);

            if (hasActiveSchedules)
                throw new InvalidOperationException("Cannot delete a shift that has future schedule assignments.");

            shiftRepo.Delete(shift);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
