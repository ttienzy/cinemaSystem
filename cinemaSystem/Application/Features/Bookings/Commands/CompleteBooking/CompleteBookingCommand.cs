using MediatR;

namespace Application.Features.Bookings.Commands.CompleteBooking
{
    public record CompleteBookingCommand(
        Guid BookingId,
        string TransactionId,
        string ReferenceCode) : IRequest<Unit>;
}
