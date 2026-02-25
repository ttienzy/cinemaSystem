using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Showtimes.EventHandlers
{
    /// <summary>
    /// Handles ShowtimeCancelledEvent:
    /// - Logs the cancellation
    /// - Future: notify affected customers, auto-cancel bookings
    /// </summary>
    public class ShowtimeCancelledEventHandler(
        ILogger<ShowtimeCancelledEventHandler> logger) : INotificationHandler<ShowtimeCancelledEvent>
    {
        public Task Handle(ShowtimeCancelledEvent e, CancellationToken ct)
        {
            logger.LogWarning(
                "Showtime {ShowtimeId} of cinema {CinemaId} for '{MovieTitle}' on {ShowDate:dd/MM/yyyy} has been cancelled.",
                e.ShowtimeId, e.CinemaId, e.MovieTitle, e.ShowDate);

            // TODO: Auto-cancel all pending bookings for this showtime
            // TODO: Send email notifications to affected customers
            // TODO: Send SignalR notification to update showtime listings

            return Task.CompletedTask;
        }
    }
}
