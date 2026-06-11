using Booking.API.Hubs.Services;
using Booking.API.Infrastructure.Caching.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Booking.API.Infrastructure.BackgroundServices;

public class SeatLockCleanupService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISeatNotificationService _seatNotificationService;
    private readonly ILogger<SeatLockCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly string _keyPrefix;

    public SeatLockCleanupService(
        IConnectionMultiplexer redis,
        ISeatNotificationService seatNotificationService,
        ILogger<SeatLockCleanupService> logger,
        IConfiguration configuration)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _seatNotificationService = seatNotificationService ?? throw new ArgumentNullException(nameof(seatNotificationService));
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
                var releasedSeatIds = await CleanupSeatMapAsync(db, seatMapKey, now);
                if (releasedSeatIds.Count == 0)
                {
                    continue;
                }

                cleanedCount += releasedSeatIds.Count;
                var showtimeId = ExtractShowtimeId(seatMapKey);
                if (showtimeId.HasValue)
                {
                    await _seatNotificationService.NotifySeatReleasedAsync(showtimeId.Value, releasedSeatIds);
                }
            }
        }

        if (cleanedCount > 0)
        {
            _logger.LogInformation("Released {Count} expired Redis seat lock(s)", cleanedCount);
        }
    }

    private static async Task<List<Guid>> CleanupSeatMapAsync(IDatabase db, RedisKey seatMapKey, DateTime now)
    {
        var entries = await db.HashGetAllAsync(seatMapKey);
        var releasedSeatIds = new List<Guid>();

        foreach (var entry in entries)
        {
            var seatData = DeserializeSeat(entry.Value);
            if (!ShouldReleaseExpiredLock(seatData, now))
            {
                continue;
            }

            seatData!.ReleaseLock();
            await db.HashSetAsync(seatMapKey, entry.Name, JsonSerializer.Serialize(seatData));
            if (TryExtractSeatId(entry.Name, out var seatId))
            {
                releasedSeatIds.Add(seatId);
            }
        }

        return releasedSeatIds;
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

    private static Guid? ExtractShowtimeId(RedisKey seatMapKey)
    {
        var parts = seatMapKey.ToString().Split(':', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < parts.Length - 1; index++)
        {
            if (string.Equals(parts[index], "showtime", StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(parts[index + 1], out var showtimeId))
            {
                return showtimeId;
            }
        }

        return null;
    }

    private static bool TryExtractSeatId(RedisValue seatField, out Guid seatId)
    {
        const string prefix = "seat:";
        var value = seatField.ToString();
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Guid.TryParse(value[prefix.Length..], out seatId);
        }

        seatId = Guid.Empty;
        return false;
    }
}
