using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using Domain.Entities.CinemaAggregate.Enum;
using MediatR;

namespace Application.Features.Cinemas.Commands.UpdateScreen
{
    public record UpdateScreenCommand(
        Guid CinemaId,
        Guid ScreenId,
        string ScreenName,
        string ScreenType,
        string Status
    ) : IRequest;

    public class UpdateScreenHandler(
        IUnitOfWork uow,
        ICinemaRepository cinemaRepo)
        : IRequestHandler<UpdateScreenCommand>
    {
        public async Task Handle(UpdateScreenCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.CinemaId, ct)
                ?? throw new KeyNotFoundException("Cinema not found");

            var screen = cinema.Screens.FirstOrDefault(s => s.Id == request.ScreenId)
                ?? throw new KeyNotFoundException("Screen not found");

            var screenType = Enum.Parse<ScreenType>(request.ScreenType, true);
            var status = Enum.Parse<ScreenStatus>(request.Status, true);

            screen.UpdateDetails(request.ScreenName, screenType, status);
            cinemaRepo.Update(cinema);
            await uow.SaveChangesAsync(ct);
        }
    }
}
