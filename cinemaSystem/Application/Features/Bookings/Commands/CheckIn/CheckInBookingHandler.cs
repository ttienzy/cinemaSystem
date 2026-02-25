using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Bookings.Commands.CheckIn
{
    public class CheckInBookingHandler(
        IBookingRepository bookingRepo,
        IUnitOfWork uow) : IRequestHandler<CheckInBookingCommand, Unit>
    {
        public async Task<Unit> Handle(CheckInBookingCommand cmd, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdForCheckInAsync(cmd.BookingId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.BookingAggregate.Booking), cmd.BookingId);

            booking.CheckIn();
            await uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
