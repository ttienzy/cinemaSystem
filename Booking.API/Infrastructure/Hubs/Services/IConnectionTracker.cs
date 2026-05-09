namespace Booking.API.Infrastructure.Hubs.Services;

/// <summary>
/// Tracks SignalR connections per showtime for viewer count and cleanup
/// </summary>
public interface IConnectionTracker
{
    /// <summary>
    /// Add a connection to a showtime
    /// </summary>
    Task AddConnectionAsync(Guid showtimeId, string connectionId, string userId);

    /// <summary>
    /// Remove a connection from a showtime
    /// </summary>
    Task RemoveConnectionAsync(Guid showtimeId, string connectionId);

    /// <summary>
    /// Remove a connection from all showtimes (on disconnect)
    /// </summary>
    Task RemoveConnectionFromAllShowtimesAsync(string connectionId);

    /// <summary>
    /// Get the number of active viewers for a showtime
    /// </summary>
    Task<int> GetViewerCountAsync(Guid showtimeId);

    /// <summary>
    /// Get all showtimes a connection is subscribed to
    /// </summary>
    Task<List<Guid>> GetShowtimesForConnectionAsync(string connectionId);
}
