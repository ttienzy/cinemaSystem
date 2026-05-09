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

/// <summary>
/// Notification payload for seat status changes
/// </summary>
public class SeatStatusChangedNotification
{
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
    public string Status { get; set; } = string.Empty; // "locked", "available", "booked"
    public string? UserId { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Notification payload for viewer count updates
/// </summary>
public class ViewerCountNotification
{
    public Guid ShowtimeId { get; set; }
    public int ViewerCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
