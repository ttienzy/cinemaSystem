namespace Booking.API.Infrastructure.Hubs.Models;

/// <summary>
/// Notification payload for viewer count updates
/// </summary>
public class ViewerCountNotification
{
    public Guid ShowtimeId { get; set; }
    public int ViewerCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
