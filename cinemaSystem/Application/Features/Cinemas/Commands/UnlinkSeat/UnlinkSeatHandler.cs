using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Common;
using Domain.Entities.CinemaAggregate;
using MediatR;

namespace Application.Features.Cinemas.Commands.UnlinkSeat
{
    public class UnlinkSeatHandler(
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<UnlinkSeatCommand, Unit>
    {
        public async Task<Unit> Handle(UnlinkSeatCommand request, CancellationToken ct)
        {
            var screen = await cinemaRepo.GetScreenWithSeatsAsync(request.ScreenId, ct)
                ?? throw new NotFoundException(nameof(Screen), request.ScreenId);

            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new NotFoundException(nameof(Seat), request.SeatId);

            if (!seat.LinkedSeatNumber.HasValue)
                throw new DomainException("Seat is not linked to any partner.");

            // Find and unlink partner seat (both seats should be unlinked)
            var partnerSeat = screen.Seats.FirstOrDefault(s => s.Number == seat.LinkedSeatNumber && s.RowName == seat.RowName);
            if (partnerSeat != null)
            {
                // Unlink the partner seat first
                partnerSeat.UnlinkCoupleSeat();
            }

            // Always unlink the primary seat
            seat.UnlinkCoupleSeat();

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
