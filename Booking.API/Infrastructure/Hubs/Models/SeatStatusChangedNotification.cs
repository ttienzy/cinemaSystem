namespace Booking.API.Infrastructure.Hubs.Models;

/// <summary>
/// Notification payload for seat status changes
/// </summary>
public class SeatStatusChangedNotification
{
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
