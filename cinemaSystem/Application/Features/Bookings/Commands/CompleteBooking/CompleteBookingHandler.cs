using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities.BookingAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingHandler(
        IBookingRepository bookingRepo,
        ISeatLockService seatLock,
        IUnitOfWork uow,
        ILogger<CompleteBookingHandler> logger) : IRequestHandler<CompleteBookingCommand, Unit>
    {
        public async Task<Unit> Handle(CompleteBookingCommand cmd, CancellationToken ct)
        {
            logger.LogInformation("Completing booking {BookingId} with Transaction {TransactionId}", cmd.BookingId, cmd.TransactionId);

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
            
            logger.LogInformation("Booking {BookingId} completed successfully.", booking.Id);
            
            return Unit.Value;
        }
    }
}
