namespace Booking.API.Infrastructure.Hubs.Interfaces;

/// <summary>
/// Client interface for BookingHub - defines methods that can be called on connected clients
/// </summary>
public interface IBookingHubClient
{
    /// <summary>
    /// Notifies client that booking has been confirmed
    /// </summary>
    Task BookingConfirmed(Guid bookingId, string status);

    /// <summary>
    /// Notifies client that booking has failed
    /// </summary>
    Task BookingFailed(Guid bookingId, string reason);
}
