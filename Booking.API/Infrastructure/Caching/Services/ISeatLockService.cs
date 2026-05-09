namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Service for managing temporary seat locks using Redis
/// </summary>
public interface ISeatLockService
{
    /// <summary>
    /// Try to lock a seat for a specific user
    /// </summary>
    Task<bool> TryLockSeatAsync(Guid showtimeId, Guid seatId, string userId, TimeSpan lockDuration);

    /// <summary>
    /// Try to lock multiple seats for a specific user
    /// </summary>
    Task<bool> TryLockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan lockDuration);

    /// <summary>
    /// Unlock a seat (when user cancels or completes booking)
    /// </summary>
    Task<bool> UnlockSeatAsync(Guid showtimeId, Guid seatId, string userId);

    /// <summary>
    /// Unlock multiple seats
    /// </summary>
    Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId);

    /// <summary>
    /// Check if a seat is currently locked
    /// </summary>
    Task<bool> IsSeatLockedAsync(Guid showtimeId, Guid seatId);

    /// <summary>
    /// Get the user who locked the seat (if any)
    /// </summary>
    Task<string?> GetSeatLockOwnerAsync(Guid showtimeId, Guid seatId);

    /// <summary>
    /// Extend lock duration for seats (when user is still in checkout process)
    /// </summary>
    Task<bool> ExtendLockAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan additionalTime);
}


