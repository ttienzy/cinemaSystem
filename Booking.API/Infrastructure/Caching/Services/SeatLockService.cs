using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Redis-based implementation of seat locking service
/// </summary>
public class SeatLockService : ISeatLockService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<SeatLockService> _logger;

    public SeatLockService(
        IDistributedCache cache,
        ILogger<SeatLockService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TryLockSeatAsync(Guid showtimeId, Guid seatId, string userId, TimeSpan lockDuration)
    {
        var lockKey = GetLockKey(showtimeId, seatId);

        // Check if already locked
        var existingLock = await _cache.GetStringAsync(lockKey);
        if (!string.IsNullOrEmpty(existingLock))
        {
            // If locked by same user, extend the lock
            if (existingLock == userId)
            {
                await _cache.SetStringAsync(lockKey, userId, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = lockDuration
                });
                _logger.LogInformation("Extended lock for seat {SeatId} in showtime {ShowtimeId} for user {UserId}",
                    seatId, showtimeId, userId);
                return true;
            }

            _logger.LogWarning("Seat {SeatId} in showtime {ShowtimeId} is already locked by another user",
                seatId, showtimeId);
            return false;
        }

        // Try to acquire lock
        await _cache.SetStringAsync(lockKey, userId, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lockDuration
        });

        _logger.LogInformation("Locked seat {SeatId} in showtime {ShowtimeId} for user {UserId} for {Duration} minutes",
            seatId, showtimeId, userId, lockDuration.TotalMinutes);

        return true;
    }

    public async Task<bool> TryLockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan lockDuration)
    {
        // Check all seats first
        foreach (var seatId in seatIds)
        {
            var lockKey = GetLockKey(showtimeId, seatId);
            var existingLock = await _cache.GetStringAsync(lockKey);

            if (!string.IsNullOrEmpty(existingLock) && existingLock != userId)
            {
                _logger.LogWarning("Cannot lock seats - seat {SeatId} is already locked by another user", seatId);
                return false;
            }
        }

        // Lock all seats
        foreach (var seatId in seatIds)
        {
            await TryLockSeatAsync(showtimeId, seatId, userId, lockDuration);
        }

        _logger.LogInformation("Locked {Count} seats in showtime {ShowtimeId} for user {UserId}",
            seatIds.Count, showtimeId, userId);

        return true;
    }

    public async Task<bool> UnlockSeatAsync(Guid showtimeId, Guid seatId, string userId)
    {
        var lockKey = GetLockKey(showtimeId, seatId);

        // Verify ownership before unlock
        var currentLock = await _cache.GetStringAsync(lockKey);
        if (string.IsNullOrEmpty(currentLock))
        {
            _logger.LogWarning("Seat {SeatId} in showtime {ShowtimeId} is not locked", seatId, showtimeId);
            return false;
        }

        if (currentLock != userId)
        {
            _logger.LogWarning("User {UserId} cannot unlock seat {SeatId} - locked by another user",
                userId, seatId);
            return false;
        }

        await _cache.RemoveAsync(lockKey);
        _logger.LogInformation("Unlocked seat {SeatId} in showtime {ShowtimeId} for user {UserId}",
            seatId, showtimeId, userId);

        return true;
    }

    public async Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        var allUnlocked = true;

        foreach (var seatId in seatIds)
        {
            var unlocked = await UnlockSeatAsync(showtimeId, seatId, userId);
            if (!unlocked)
            {
                allUnlocked = false;
            }
        }

        return allUnlocked;
    }

    public async Task<bool> IsSeatLockedAsync(Guid showtimeId, Guid seatId)
    {
        var lockKey = GetLockKey(showtimeId, seatId);
        var lockValue = await _cache.GetStringAsync(lockKey);
        return !string.IsNullOrEmpty(lockValue);
    }

    public async Task<string?> GetSeatLockOwnerAsync(Guid showtimeId, Guid seatId)
    {
        var lockKey = GetLockKey(showtimeId, seatId);
        return await _cache.GetStringAsync(lockKey);
    }

    public async Task<bool> ExtendLockAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan additionalTime)
    {
        foreach (var seatId in seatIds)
        {
            var lockKey = GetLockKey(showtimeId, seatId);
            var currentLock = await _cache.GetStringAsync(lockKey);

            if (currentLock != userId)
            {
                _logger.LogWarning("Cannot extend lock - user {UserId} does not own seat {SeatId}", userId, seatId);
                return false;
            }

            await _cache.SetStringAsync(lockKey, userId, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = additionalTime
            });
        }

        _logger.LogInformation("Extended lock for {Count} seats in showtime {ShowtimeId} for user {UserId}",
            seatIds.Count, showtimeId, userId);

        return true;
    }

    private static string GetLockKey(Guid showtimeId, Guid seatId)
    {
        return $"seat-lock:{showtimeId}:{seatId}";
    }
}


