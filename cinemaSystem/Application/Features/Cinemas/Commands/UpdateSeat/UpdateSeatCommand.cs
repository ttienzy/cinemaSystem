using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Cinemas.Commands.UpdateSeat
{
    public record UpdateSeatCommand(
        Guid CinemaId,
        Guid ScreenId,
        Guid SeatId,
        Guid SeatTypeId,
        string RowName,
        int Number,
        bool IsActive
    ) : IRequest;

    public class UpdateSeatHandler(
        IUnitOfWork uow,
        ICinemaRepository cinemaRepo)
        : IRequestHandler<UpdateSeatCommand>
    {
        public async Task Handle(UpdateSeatCommand request, CancellationToken ct)
        {
            var cinema = await cinemaRepo.GetByIdAsync(request.CinemaId, ct)
                ?? throw new KeyNotFoundException("Cinema not found");

            var screen = cinema.Screens.FirstOrDefault(s => s.Id == request.ScreenId)
                ?? throw new KeyNotFoundException("Screen not found");

            var seat = screen.Seats.FirstOrDefault(s => s.Id == request.SeatId)
                ?? throw new KeyNotFoundException("Seat not found");

            seat.UpdateDetails(request.SeatTypeId, request.RowName, request.Number, request.IsActive, seat.IsBlocked);
            cinemaRepo.Update(cinema);
            await uow.SaveChangesAsync(ct);
        }
    }
}
