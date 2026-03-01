using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Cinemas.Commands.CreateSeatsBulk
{
    public record CreateSeatsBulkCommand(Guid ScreenId, List<SeatGenerateRequest> Requests) : IRequest<List<Guid>>;

    public class CreateSeatsBulkHandler(ICinemaRepository cinemaRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<CreateSeatsBulkCommand, List<Guid>>
    {
        public async Task<List<Guid>> Handle(CreateSeatsBulkCommand request, CancellationToken ct)
        {
            var screen = await cinemaRepo.GetScreenByIdAsync(request.ScreenId, ct)
                ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Screen), request.ScreenId);

            var seats = request.Requests.Select(r => new Seat(
                r.SeatTypeId,
                r.RowName,
                r.Number,
                r.IsActive,
                r.IsBlocked,
                request.ScreenId
            )).ToList();

            screen.AddSeats(seats);
            
            // Note: Since screen is usually tracked as part of the Cinema aggregate, 
            // we should ideally get the cinema and update it, but ICinemaRepository 
            // has Update(Cinema) and Screen is likely tracked.
            // Let's assume the DbContext will track changes to the screen's seat collection.
            
            await unitOfWork.SaveChangesAsync(ct);

            return seats.Select(s => s.Id).ToList();
        }
    }
}
