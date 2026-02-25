using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using MediatR;

namespace Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingHandler(
        IBookingRepository bookingRepo,
        ISeatLockService seatLock,
        IUnitOfWork uow) : IRequestHandler<CompleteBookingCommand, Unit>
    {
        public async Task<Unit> Handle(CompleteBookingCommand cmd, CancellationToken ct)
        {
            var booking = await bookingRepo.GetByIdWithDetailsAsync(cmd.BookingId, ct)
                ?? throw new NotFoundException(nameof(Booking), cmd.BookingId);

            // Create payment and complete booking (raises BookingCompletedEvent)
            var payment = Payment.CreatePending(booking.Id, booking.FinalAmount);
            payment.Complete(cmd.TransactionId, cmd.ReferenceCode);
            booking.Complete(payment);

            // Release Redis locks (seats are now officially booked in DB)
            var seatIds = booking.BookingTickets.Select(t => t.SeatId).ToList();
            await seatLock.ReleaseSeatsAsync(booking.ShowtimeId, seatIds, ct);

            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
