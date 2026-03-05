using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;

namespace Application.Features.Shared.TimeSlots.Commands
{
    // === Command Records ===

    /// <summary>Tạo khung giờ mới.</summary>
    public record CreateTimeSlotCommand(TimeSpan StartTime, TimeSpan EndTime, string dateType) : IRequest<Guid>;

    /// <summary>Cập nhật khung giờ.</summary>
    public record UpdateTimeSlotCommand(Guid Id, string Name, TimeSpan StartTime, TimeSpan EndTime, string dateType, bool isActive) : IRequest;

    /// <summary>Xóa khung giờ.</summary>
    public record DeleteTimeSlotCommand(Guid Id) : IRequest;

    // === Handlers ===

    /// <summary>Handler tạo khung giờ.</summary>
    public class CreateTimeSlotHandler(
        ITimeSlotRepository repo, IUnitOfWork uow)
        : IRequestHandler<CreateTimeSlotCommand, Guid>
    {
        public async Task<Guid> Handle(CreateTimeSlotCommand request, CancellationToken ct)
        {
            var slot = new TimeSlot( request.StartTime, request.EndTime, request.dateType, true);
            await repo.AddAsync(slot, ct);
            await uow.SaveChangesAsync(ct);
            return slot.Id;
        }
    }

    /// <summary>Handler cập nhật khung giờ.</summary>
    public class UpdateTimeSlotHandler(
        ITimeSlotRepository repo, IUnitOfWork uow)
        : IRequestHandler<UpdateTimeSlotCommand>
    {
        public async Task Handle(UpdateTimeSlotCommand request, CancellationToken ct)
        {
            var slot = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy khung giờ ID: {request.Id}");
            slot.UpdateTimeSlot(request.StartTime, request.EndTime, request.dateType, request.isActive);
            repo.Update(slot);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Handler xóa khung giờ.</summary>
    public class DeleteTimeSlotHandler(
        ITimeSlotRepository repo, IUnitOfWork uow)
        : IRequestHandler<DeleteTimeSlotCommand>
    {
        public async Task Handle(DeleteTimeSlotCommand request, CancellationToken ct)
        {
            var slot = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy khung giờ ID: {request.Id}");
            repo.Delete(slot);
            await uow.SaveChangesAsync(ct);
        }
    }
}
