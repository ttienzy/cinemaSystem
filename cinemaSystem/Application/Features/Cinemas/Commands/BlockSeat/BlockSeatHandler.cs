using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using MediatR;

namespace Application.Features.Cinemas.Commands.BlockSeat
{
    public record BlockSeatCommand(Guid ScreenId, Guid SeatId, string Reason) : IRequest<Unit>;

    public class BlockSeatHandler(
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<BlockSeatCommand, Unit>
    {
        public async Task<Unit> Handle(BlockSeatCommand request, CancellationToken ct)
        {
            var screen = await cinemaRepo.GetScreenWithSeatsAsync(request.ScreenId, ct)
                ?? throw new NotFoundException(nameof(Screen), request.ScreenId);

            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new NotFoundException(nameof(Seat), request.SeatId);

            seat.Block(request.Reason);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
