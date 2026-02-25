using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using MediatR;

namespace Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingHandler(
        IBookingRepository bookingRepo,
        ISeatLockService seatLock,
        IUnitOfWork uow) : IRequestHandler<CancelBookingCommand, Unit>
    {
        public async Task<Unit> Handle(CancelBookingCommand cmd, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdWithDetailsAsync(cmd.BookingId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.BookingAggregate.Booking), cmd.BookingId);

            // Raises BookingCancelledEvent internally
            booking.Cancel(cmd.Reason);

            // Release Redis seat locks
            var seatIds = booking.BookingTickets.Select(t => t.SeatId).ToList();
            await seatLock.ReleaseSeatsAsync(booking.ShowtimeId, seatIds, ct);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
