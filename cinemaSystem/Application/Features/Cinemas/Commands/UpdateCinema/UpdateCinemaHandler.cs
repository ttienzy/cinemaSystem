using Application.Common.Interfaces.Persistence;
using Application.Common.Exceptions;
using Domain.Entities.CinemaAggregate;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Commands.UpdateCinema
{
    public record UpdateCinemaCommand(Guid Id, CinemaUpsertRequest Request) : IRequest;

    public class UpdateCinemaHandler(ICinemaRepository cinemaRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<UpdateCinemaCommand>
    {
        public async Task Handle(UpdateCinemaCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Cinema), request.Id);

            cinema.UpdateDetails(
                request.Request.CinemaName,
                request.Request.Address,
                request.Request.Phone,
                request.Request.Email,
                request.Request.Image,
                request.Request.ManagerName,
                request.Request.Status
            );

            cinemaRepo.Update(cinema);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
