using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.Commands.CancelExpiredBookings
{
    public class CancelExpiredBookingsHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        ISeatLockService seatLock,
        IUnitOfWork uow,
        ILogger<CancelExpiredBookingsHandler> logger) : IRequestHandler<CancelExpiredBookingsCommand, int>
    {
        public async Task<int> Handle(CancelExpiredBookingsCommand request, CancellationToken ct)
        {
            var expiredBookings = await bookingRepo.GetExpiredPendingAsync(ct);
            var cancelledCount = 0;

            foreach (var booking in expiredBookings)
            {
                try
                {
                    // Release Redis locks
                    var seatIds = booking.BookingTickets.Select(t => t.SeatId).ToList();
                    await seatLock.ReleaseSeatsAsync(booking.ShowtimeId, seatIds, ct);

                    // Decrement booked seats on showtime
                    var showtime = await showtimeRepo.GetByIdAsync(booking.ShowtimeId, ct);
                    if (showtime != null)
                    {
                        showtime.DecrementBookedSeats(seatIds.Count);
                    }

                    // Cancel the booking
                    booking.Cancel("Auto-cancelled: Payment expired");

                    await uow.SaveChangesAsync(ct);
                    cancelledCount++;

                    logger.LogInformation("Auto-cancelled expired booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to cancel expired booking {BookingId}", booking.Id);
                }
            }

            return cancelledCount;
        }
    }
}
