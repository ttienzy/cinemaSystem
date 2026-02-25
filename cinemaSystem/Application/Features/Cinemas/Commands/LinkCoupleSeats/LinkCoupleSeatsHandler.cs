using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Cinemas.Commands.LinkCoupleSeats
{
    public class LinkCoupleSeatsHandler(
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<LinkCoupleSeatsCommand, Unit>
    {
        public async Task<Unit> Handle(LinkCoupleSeatsCommand cmd, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdWithScreensAndSeatsAsync(cmd.CinemaId, ct)
                ?? throw new NotFoundException("Cinema", cmd.CinemaId);

            var screen = cinema.Screens.FirstOrDefault(s => s.Id == cmd.ScreenId)
                ?? throw new NotFoundException("Screen", cmd.ScreenId);

            // Domain logic inside Screen aggregate
            screen.LinkCoupleSeats(cmd.RowName, cmd.SeatNumber1, cmd.SeatNumber2);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
