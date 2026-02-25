using MediatR;

namespace Application.Features.Bookings.Commands.CancelBooking
{
    public record CancelBookingCommand(Guid BookingId, string Reason = "User cancelled") : IRequest<Unit>;
}
