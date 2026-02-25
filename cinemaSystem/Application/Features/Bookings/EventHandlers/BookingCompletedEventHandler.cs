using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Bookings.EventHandlers
{
    /// <summary>
    /// Handles BookingCompletedEvent:
    /// - Sends confirmation email
    /// - Logs the completed booking
    /// - Future: Send SignalR update, generate QR code
    /// </summary>
    public class BookingCompletedEventHandler(
        ILogger<BookingCompletedEventHandler> logger) : INotificationHandler<BookingCompletedEvent>
    {
        public Task Handle(BookingCompletedEvent e, CancellationToken ct)
        {
            logger.LogInformation(
                "Booking {BookingId} completed! Code: {BookingCode}. " +
                "Amount: {Amount:N0} VND. Customer: {CustomerId}",
                e.BookingId, e.BookingCode, e.FinalAmount, e.CustomerId);

            // TODO: Send confirmation email with booking code + QR
            // TODO: Send SignalR notification to update seat map
            // TODO: Generate e-ticket PDF

            return Task.CompletedTask;
        }
    }
}
