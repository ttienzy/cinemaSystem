using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using MediatR;
using Shared.Models.DataModels.CinemaDtos;

namespace Application.Features.Cinemas.Commands.CreateScreen
{
    public record CreateScreenCommand(Guid CinemaId, ScreenRequest Request) : IRequest<Guid>;

    public class CreateScreenHandler(ICinemaRepository cinemaRepo, IUnitOfWork unitOfWork) 
        : IRequestHandler<CreateScreenCommand, Guid>
    {
        public async Task<Guid> Handle(CreateScreenCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdWithScreensAsync(request.CinemaId, ct)
                ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Cinema), request.CinemaId);

            var screen = new Screen(
                request.CinemaId,
                request.Request.ScreenName,
                request.Request.Type,
                request.Request.Status
            );

            cinema.AddItem(screen);

            cinemaRepo.Update(cinema);
            await unitOfWork.SaveChangesAsync(ct);

            return screen.Id;
        }
    }
}
