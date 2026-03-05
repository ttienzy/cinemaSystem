using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Common;
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

            // Step 1: Get the seat to be linked
            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new NotFoundException(nameof(Seat), request.SeatId);

            // Step 2: Get partner seat by seat number
            var partnerSeat = screen.Seats.FirstOrDefault(s => s.Number == request.PartnerSeatNumber && s.RowName == seat.RowName)
                ?? throw new NotFoundException("Partner seat", $"number {request.PartnerSeatNumber}");

            // Step 3: Validate SeatTypeId match (both must be same seat type)
            if (seat.SeatTypeId != partnerSeat.SeatTypeId)
                throw new DomainException(
                    $"Cannot link seats with different types. Seat {seat.SeatLabel} is type '{seat.SeatTypeId}' but seat {partnerSeat.SeatLabel} is type '{partnerSeat.SeatTypeId}'.");

            // Step 4: Validate adjacency (must be adjacent seats)
            if (!seat.CanLinkAsCouple(request.PartnerSeatNumber))
                throw new DomainException("Couple seats must be adjacent (consecutive numbers).");

            // Step 5: Check if either seat is already linked
            if (seat.LinkedSeatNumber.HasValue)
                throw new DomainException($"Seat {seat.SeatLabel} is already linked to seat {seat.LinkedSeatNumber}.");
            if (partnerSeat.LinkedSeatNumber.HasValue)
                throw new DomainException($"Seat {partnerSeat.SeatLabel} is already linked to seat {partnerSeat.LinkedSeatNumber}.");

            // Step 6: Bidirectional link
            seat.LinkWithSeat(request.PartnerSeatNumber);
            partnerSeat.LinkWithSeat(seat.Number);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
