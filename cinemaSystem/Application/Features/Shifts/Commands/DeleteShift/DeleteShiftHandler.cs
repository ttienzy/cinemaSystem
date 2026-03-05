using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Shifts.Commands.DeleteShift
{
    /// <summary>
    /// Xóa ca làm — kiểm tra không có lịch làm đang active trước khi xóa.
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
                ?? throw new KeyNotFoundException($"Không tìm thấy ca làm với ID: {request.ShiftId}");

            // Kiểm tra có lịch làm trong tương lai không
            var hasActiveSchedules = scheduleRepo.GetQueryable()
                .Any(ws => ws.ShiftId == request.ShiftId && ws.WorkDate >= DateTime.UtcNow.Date);

            if (hasActiveSchedules)
                throw new InvalidOperationException("Không thể xóa ca làm đang có lịch phân công trong tương lai.");

            shiftRepo.Delete(shift);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
