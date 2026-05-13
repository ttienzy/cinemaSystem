using Booking.API.Infrastructure.Hubs.Models;

namespace Booking.API.Infrastructure.Hubs.Interfaces;

/// <summary>
/// Defines client-side methods that can be invoked from the server
/// Type-safe interface for SignalR client callbacks
/// </summary>
public interface ISeatHubClient
{
    /// <summary>
    /// Notifies clients when seats are locked by a user
    /// </summary>
    Task SeatLocked(SeatStatusChangedNotification notification);

    /// <summary>
    /// Notifies clients when seats are unlocked
    /// </summary>
    Task SeatUnlocked(SeatStatusChangedNotification notification);

    /// <summary>
    /// Notifies clients when seats are booked (confirmed)
    /// </summary>
    Task SeatBooked(SeatStatusChangedNotification notification);

    /// <summary>
    /// Notifies clients when seats are released (from expired bookings)
    /// </summary>
    Task SeatReleased(SeatStatusChangedNotification notification);

    /// <summary>
    /// Notifies clients about the current viewer count for the showtime
    /// </summary>
    Task ViewerCountUpdated(ViewerCountNotification notification);
}
