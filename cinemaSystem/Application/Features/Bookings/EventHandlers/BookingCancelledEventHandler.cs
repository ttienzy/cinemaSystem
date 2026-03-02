using Application.Common.Interfaces.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.EventHandlers
{
    /// <summary>
    /// Handles BookingCancelledEvent:
    /// - Logs the cancellation
    /// - Future: Send SignalR update to release seats on seat map
    /// - Future: Send cancellation email to customer
    /// </summary>
    public class BookingCancelledEventHandler(
        ILogger<BookingCancelledEventHandler> logger,
        ISeatNotificationService seatNotificationService) : INotificationHandler<BookingCancelledEvent>
    {
        public async Task Handle(BookingCancelledEvent e, CancellationToken ct)
        {
            logger.LogInformation(
                "Booking {BookingId} cancelled. Reason: {Reason}. " +
                "Customer: {CustomerId}. Seats released: [{Seats}]",
                e.BookingId, e.Reason, e.CustomerId,
                string.Join(", ", e.SeatIds));

            // Send SignalR notification to update seat map
            await seatNotificationService.NotifySeatReleasedAsync(e.ShowtimeId, e.SeatIds);

            // TODO: Send cancellation email
        }
    }
}
