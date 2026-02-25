using MediatR;

namespace Application.Features.Bookings.Commands.CheckIn
{
    public record CheckInBookingCommand(Guid BookingId) : IRequest<Unit>;
}
