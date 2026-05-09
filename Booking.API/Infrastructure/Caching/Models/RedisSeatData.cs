using Booking.API.Application.DTOs.Responses;

namespace Booking.API.Infrastructure.Caching.Models;

/// <summary>
/// Data structure stored in Redis for each seat
/// Serialized as JSON in Redis Hash
/// </summary>
public class RedisSeatData
{
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
}


