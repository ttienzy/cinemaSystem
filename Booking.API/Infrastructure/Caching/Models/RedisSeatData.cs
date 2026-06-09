namespace Booking.API.Infrastructure.Caching.Models;

/// <summary>
/// Data structure stored in Redis for each seat
/// Serialized as JSON in Redis Hash
/// </summary>
public class RedisSeatData
{
    public Guid SeatId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public decimal Price { get; set; }
    public SeatStatus Status { get; set; }
    public string? UserId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? BookedAt { get; set; }

    public bool IsLockExpired()
    {
        if (Status != SeatStatus.Locked || !LockedUntil.HasValue)
            return false;

        return DateTime.UtcNow > LockedUntil.Value;
    }

    public bool IsOwnedBy(string userId)
    {
        return UserId == userId;
    }

    public void ReleaseLock()
    {
        Status = SeatStatus.Available;
        UserId = null;
        LockedAt = null;
        LockedUntil = null;
    }

    public void MarkBooked(Guid bookingId)
    {
        Status = SeatStatus.Booked;
        BookingId = bookingId;
        BookedAt = DateTime.UtcNow;
        LockedAt = null;
        LockedUntil = null;
    }

    public void ReleaseBooking()
    {
        Status = SeatStatus.Available;
        UserId = null;
        BookingId = null;
        BookedAt = null;
        LockedAt = null;
        LockedUntil = null;
    }
}
public enum SeatStatus
{
    Available,
    Locked,
    Booked,
    Unavailable
}
