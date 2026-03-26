using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;

namespace Application.Features.Shared.TimeSlots.Commands
{
    // === Command Records ===

    /// <summary>Create a new time slot.</summary>
    public record CreateTimeSlotCommand(TimeSpan StartTime, TimeSpan EndTime, string dateType) : IRequest<Guid>;

    /// <summary>Update time slot.</summary>
    public record UpdateTimeSlotCommand(Guid Id, string Name, TimeSpan StartTime, TimeSpan EndTime, string dateType, bool isActive) : IRequest;

    /// <summary>Delete time slot.</summary>
    public record DeleteTimeSlotCommand(Guid Id) : IRequest;

    // === Handlers ===

    /// <summary>Handler for creating time slots.</summary>
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

    /// <summary>Handler for updating time slots.</summary>
    public class UpdateTimeSlotHandler(
        ITimeSlotRepository repo, IUnitOfWork uow)
        : IRequestHandler<UpdateTimeSlotCommand>
    {
        public async Task Handle(UpdateTimeSlotCommand request, CancellationToken ct)
        {
            var slot = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Time slot not found with ID: {request.Id}");
            slot.UpdateTimeSlot(request.StartTime, request.EndTime, request.dateType, request.isActive);
            repo.Update(slot);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Handler for deleting time slots.</summary>
    public class DeleteTimeSlotHandler(
        ITimeSlotRepository repo, IUnitOfWork uow)
        : IRequestHandler<DeleteTimeSlotCommand>
    {
        public async Task Handle(DeleteTimeSlotCommand request, CancellationToken ct)
        {
            var slot = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Time slot not found with ID: {request.Id}");
            repo.Delete(slot);
            await uow.SaveChangesAsync(ct);
        }
    }
}
