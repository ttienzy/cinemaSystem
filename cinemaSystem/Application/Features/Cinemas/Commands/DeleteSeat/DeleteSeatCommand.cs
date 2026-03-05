using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Cinemas.Commands.DeleteSeat
{
    public record DeleteSeatCommand(Guid CinemaId, Guid ScreenId, Guid SeatId) : IRequest;

    public class DeleteSeatHandler(
        IUnitOfWork uow,
        ICinemaRepository cinemaRepo)
        : IRequestHandler<DeleteSeatCommand>
    {
        public async Task Handle(DeleteSeatCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.CinemaId, ct)
                ?? throw new KeyNotFoundException("Cinema not found");

            var screen = cinema.Screens.FirstOrDefault(s => s.Id == request.ScreenId)
                ?? throw new KeyNotFoundException("Screen not found");

            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new KeyNotFoundException("Seat not found");

            // Soft delete - mark as inactive
            seat.UpdateDetails(seat.SeatTypeId, seat.RowName, seat.Number, false, seat.IsBlocked);
            cinemaRepo.Update(cinema);
            await uow.SaveChangesAsync(ct);
        }
    }
}
