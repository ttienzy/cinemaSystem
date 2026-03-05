using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Shifts.Commands.UpdateShift
{
    /// <summary>
    /// Cập nhật ca làm — thay đổi tên, giờ bắt đầu/kết thúc.
    /// </summary>
    public record UpdateShiftCommand(Guid ShiftId, ShiftUpsertRequest Request) : IRequest;

    public class UpdateShiftHandler(
        IShiftRepository shiftRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateShiftCommand>
    {
        public async Task Handle(UpdateShiftCommand request, CancellationToken ct)
        {
            var shift = await shiftRepo.GetByIdAsync(request.ShiftId, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy ca làm với ID: {request.ShiftId}");

            // Sử dụng domain method UpdateShift
            shift.UpdateShift(
                request.Request.CinemaId,
                request.Request.Name,
                request.Request.DefaultStartTime,
                request.Request.DefaultEndTime);

            shiftRepo.Update(shift);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
