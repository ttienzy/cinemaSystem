using Application.Common.Interfaces.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.EventHandlers
{
    /// <summary>
    /// Handles BookingCreatedEvent:
    /// - Logs the booking creation
    /// - Future: Send real-time notification via SignalR
    /// </summary>
    public class BookingCreatedEventHandler(
        ILogger<BookingCreatedEventHandler> logger) : INotificationHandler<BookingCreatedEvent>
    {
        public Task Handle(BookingCreatedEvent e, CancellationToken ct)
        {
            logger.LogInformation(
                "Booking {BookingId} created for customer {CustomerId}, showtime {ShowtimeId}. " +
                "Amount: {Amount:N0} VND. Expires at {ExpiresAt}. Seats: [{Seats}]",
                e.BookingId, e.CustomerId, e.ShowtimeId,
                e.TotalAmount, e.ExpiresAt,
                string.Join(", ", e.SeatIds));

            // TODO: Send SignalR notification to update seat map in real-time
            // TODO: Schedule expiry job (Hangfire / background task)

            return Task.CompletedTask;
        }
    }
}
