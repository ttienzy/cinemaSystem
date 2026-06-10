using Booking.API.Infrastructure.Caching.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Booking.API.Infrastructure.BackgroundServices;

public class SeatLockCleanupService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SeatLockCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly string _keyPrefix;

    public SeatLockCleanupService(
        IConnectionMultiplexer redis,
        ILogger<SeatLockCleanupService> logger,
        IConfiguration configuration)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var intervalSeconds = configuration.GetValue<int>(
            "BackgroundServices:SeatLockCleanupIntervalSeconds",
            30);

        _cleanupInterval = TimeSpan.FromSeconds(intervalSeconds);
        _keyPrefix = configuration.GetValue<string>("Redis:KeyPrefix") ?? "cinema";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SeatLockCleanupService started with interval {IntervalSeconds}s",
            _cleanupInterval.TotalSeconds);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSeatLocksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired Redis seat locks");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredSeatLocksAsync(CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var cleanedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var endpoint in _redis.GetEndPoints())
        {
            var server = _redis.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var keys = server.Keys(
                database: db.Database,
                pattern: $"{_keyPrefix}:showtime:*:seats");

            foreach (var seatMapKey in keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cleanedCount += await CleanupSeatMapAsync(db, seatMapKey, now);
            }
        }

        if (cleanedCount > 0)
        {
            _logger.LogInformation("Released {Count} expired Redis seat lock(s)", cleanedCount);
        }
    }

    private static async Task<int> CleanupSeatMapAsync(IDatabase db, RedisKey seatMapKey, DateTime now)
    {
        var entries = await db.HashGetAllAsync(seatMapKey);
        var cleanedCount = 0;

        foreach (var entry in entries)
        {
            var seatData = DeserializeSeat(entry.Value);
            if (!ShouldReleaseExpiredLock(seatData, now))
            {
                continue;
            }

            seatData!.ReleaseLock();
            await db.HashSetAsync(seatMapKey, entry.Name, JsonSerializer.Serialize(seatData));
            cleanedCount++;
        }

        return cleanedCount;
    }

    private static bool ShouldReleaseExpiredLock(RedisSeatData? seatData, DateTime now)
        => seatData is
        {
            Status: SeatStatus.Locked,
            BookingId: null,
            LockedUntil: not null
        } && seatData.LockedUntil.Value <= now;

    private static RedisSeatData? DeserializeSeat(RedisValue value)
        => value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<RedisSeatData>((string)value!);
}
