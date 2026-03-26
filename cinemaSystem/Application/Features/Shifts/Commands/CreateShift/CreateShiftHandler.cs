using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using MediatR;
using Shared.Models.DataModels.StaffDtos;

namespace Application.Features.Shifts.Commands.CreateShift
{
    /// <summary>
    /// Create a new cinema shift — Manager creates shifts: "Morning Shift", "Afternoon Shift", "Evening Shift".
    /// </summary>
    public record CreateShiftCommand(ShiftUpsertRequest Request) : IRequest<Guid>;

    public class CreateShiftHandler(
        IShiftRepository shiftRepo,
        IUnitOfWork unitOfWork)
        : IRequestHandler<CreateShiftCommand, Guid>
    {
        public async Task<Guid> Handle(CreateShiftCommand request, CancellationToken ct)
        {
            var shift = new Shift(
                request.Request.CinemaId,
                request.Request.Name,
                request.Request.DefaultStartTime,
                request.Request.DefaultEndTime);

            await shiftRepo.AddAsync(shift, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return shift.Id;
        }
    }
}
