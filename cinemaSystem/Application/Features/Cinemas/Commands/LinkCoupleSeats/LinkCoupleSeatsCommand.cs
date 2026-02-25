using MediatR;

namespace Application.Features.Cinemas.Commands.LinkCoupleSeats
{
    public record LinkCoupleSeatsCommand(
        Guid CinemaId,
        Guid ScreenId,
        string RowName,
        int SeatNumber1,
        int SeatNumber2) : IRequest<Unit>;
}
