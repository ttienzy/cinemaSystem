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
        ILogger<BookingCancelledEventHandler> logger) : INotificationHandler<BookingCancelledEvent>
    {
        public Task Handle(BookingCancelledEvent e, CancellationToken ct)
        {
            logger.LogInformation(
                "Booking {BookingId} cancelled. Reason: {Reason}. " +
                "Customer: {CustomerId}. Seats released: [{Seats}]",
                e.BookingId, e.Reason, e.CustomerId,
                string.Join(", ", e.SeatIds));

            // TODO: Send SignalR notification to update seat map
            // TODO: Send cancellation email

            return Task.CompletedTask;
        }
    }
}
