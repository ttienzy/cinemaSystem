namespace Booking.API.Infrastructure.Hubs.Services;

/// <summary>
/// Service for broadcasting seat status changes to SignalR clients
/// Abstraction layer between business logic and SignalR Hub
/// </summary>
public interface ISeatNotificationService
{
    /// <summary>
    /// Broadcast seat locked notification
    /// </summary>
    Task NotifySeatLockedAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string userId,
        DateTime lockedUntil);

    /// <summary>
    /// Broadcast seat unlocked notification
    /// </summary>
    Task NotifySeatUnlockedAsync(
        Guid showtimeId,
        List<Guid> seatIds);

    /// <summary>
    /// Broadcast seat booked notification
    /// </summary>
    Task NotifySeatBookedAsync(
        Guid showtimeId,
        List<Guid> seatIds);

    /// <summary>
    /// Broadcast seat released notification (from expired bookings)
    /// </summary>
    Task NotifySeatReleasedAsync(
        Guid showtimeId,
        List<Guid> seatIds);
}
