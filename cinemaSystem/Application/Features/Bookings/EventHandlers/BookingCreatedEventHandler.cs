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
        ILogger<BookingCreatedEventHandler> logger,
        ISeatNotificationService seatNotificationService) : INotificationHandler<BookingCreatedEvent>
    {
        public async Task Handle(BookingCreatedEvent e, CancellationToken ct)
        {
            logger.LogInformation(
                "Booking {BookingId} created for customer {CustomerId}, showtime {ShowtimeId}. " +
                "Amount: {Amount:N0} VND. Expires at {ExpiresAt}. Seats: [{Seats}]",
                e.BookingId, e.CustomerId, e.ShowtimeId,
                e.TotalAmount, e.ExpiresAt,
                string.Join(", ", e.SeatIds));

            // Send SignalR notification to update seat map in real-time
            await seatNotificationService.NotifySeatReservedAsync(e.ShowtimeId, e.SeatIds);
            
            // TODO: Schedule expiry job (Hangfire / background task)
        }
    }
}
