using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using Domain.Entities.CinemaAggregate.Enum;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Commands.CreateCinema
{
    public record CreateCinemaCommand(CinemaUpsertRequest Request) : IRequest<Guid>;

    public class CreateCinemaHandler(ICinemaRepository cinemaRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<CreateCinemaCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCinemaCommand request, CancellationToken ct)
        {
            var cinema = new Cinema(
                request.Request.CinemaName,
                request.Request.Address,
                request.Request.Phone,
                request.Request.Email,
                request.Request.Image,
                request.Request.ManagerName,
                request.Request.Status
            );

            await cinemaRepo.AddAsync(cinema, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return cinema.Id;
        }
    }
}
