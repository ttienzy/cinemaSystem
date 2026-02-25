using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.CinemaAggregate;
using MediatR;

namespace Application.Features.Cinemas.Commands.LinkSeat
{
    public record LinkSeatCommand(Guid ScreenId, Guid SeatId, int PartnerSeatNumber) : IRequest<Unit>;

    public class LinkSeatHandler(
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<LinkSeatCommand, Unit>
    {
        public async Task<Unit> Handle(LinkSeatCommand request, CancellationToken ct)
        {
            var screen = await cinemaRepo.GetScreenWithSeatsAsync(request.ScreenId, ct)
                ?? throw new NotFoundException(nameof(Screen), request.ScreenId);

            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new NotFoundException(nameof(Seat), request.SeatId);

            // Logic: A 10+ year dev ensures the partner also exists in the same screen
            if (!screen.Seats.Any(s => s.Number == request.PartnerSeatNumber))
                throw new ValidationException("PartnerSeatNumber", $"Seat number {request.PartnerSeatNumber} not found in this screen.");

            seat.LinkWithSeat(request.PartnerSeatNumber);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
