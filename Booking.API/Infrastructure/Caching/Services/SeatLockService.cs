using Booking.API.Infrastructure.Caching.Helpers;
using StackExchange.Redis;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Redis-based implementation of seat locking service
/// </summary>
public class SeatLockService : ISeatLockService
{
    private readonly IDatabase _redis;
    private readonly ILogger<SeatLockService> _logger;

    public SeatLockService(
        IConnectionMultiplexer redis,
        ILogger<SeatLockService> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TryLockSeatAsync(Guid showtimeId, Guid seatId, string userId, TimeSpan lockDuration)
    {
        var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);

        // Check if already locked
        var acquired = await _redis.StringSetAsync(
            key: lockKey,
            value: userId,
            expiry: lockDuration,
            when: When.NotExists
        );

        if (acquired)
        {
            _logger.LogInformation(
                "Locked seat {SeatId} in showtime {ShowtimeId} for user {UserId}",
                seatId, showtimeId, userId);

            return true;
        }


        var existingLock = await _redis.StringGetAsync(lockKey);

        if (existingLock == userId)
        {
            await _redis.KeyExpireAsync(lockKey, lockDuration);

            _logger.LogInformation(
                "Extended lock for seat {SeatId} in showtime {ShowtimeId} for user {UserId}",
                seatId, showtimeId, userId);

            return true;
        }

        _logger.LogWarning(
            "Seat {SeatId} in showtime {ShowtimeId} is already locked by another user",
            seatId, showtimeId);

        return false;
    }

    public async Task<bool> TryLockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan lockDuration)
    {
        // Check all seats first
        foreach (var seatId in seatIds)
        {
            var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);
            var existingLock = await _redis.StringGetAsync(lockKey);

            if (existingLock.HasValue && existingLock != userId)
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
        var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);

        // Verify ownership before unlock
        var currentLock = await _redis.StringGetAsync(lockKey);
        if (!currentLock.HasValue)
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

        await _redis.KeyDeleteAsync(lockKey);
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
        var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);
        var lockValue = await _redis.StringGetAsync(lockKey);
        return lockValue.HasValue;
    }

    public async Task<string?> GetSeatLockOwnerAsync(Guid showtimeId, Guid seatId)
    {
        var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);
        var lockValue = await _redis.StringGetAsync(lockKey);
        return lockValue.HasValue ? lockValue.ToString() : null;
    }

    public async Task<bool> ExtendLockAsync(Guid showtimeId, List<Guid> seatIds, string userId, TimeSpan additionalTime)
    {
        foreach (var seatId in seatIds)
        {
            var lockKey = RedisHelper.GetLockKey(showtimeId, seatId);
            var currentLock = await _redis.StringGetAsync(lockKey);

            if (currentLock != userId)
            {
                _logger.LogWarning("Cannot extend lock - user {UserId} does not own seat {SeatId}", userId, seatId);
                return false;
            }

            await _redis.KeyExpireAsync(lockKey, additionalTime);
        }

        _logger.LogInformation("Extended lock for {Count} seats in showtime {ShowtimeId} for user {UserId}",
            seatIds.Count, showtimeId, userId);

        return true;
    }

}
