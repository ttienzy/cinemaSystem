using MediatR;

namespace Application.Features.Cinemas.Commands.UnlinkSeat
{
    public record UnlinkSeatCommand(Guid ScreenId, Guid SeatId) : IRequest<Unit>;
}
