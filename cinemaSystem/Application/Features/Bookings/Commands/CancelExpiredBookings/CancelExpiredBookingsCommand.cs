using MediatR;

namespace Application.Features.Bookings.Commands.CancelExpiredBookings
{
    public record CancelExpiredBookingsCommand : IRequest<int>;
}
