using Booking.API.Infrastructure.Hubs.Builders;
using Booking.API.Infrastructure.Hubs.Constants;
using StackExchange.Redis;

namespace Booking.API.Infrastructure.Hubs.Services;

/// <summary>
/// Redis-based implementation of connection tracking
/// Uses Redis Sets for efficient membership tracking
/// </summary>
public class RedisConnectionTracker : IConnectionTracker
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisConnectionTracker> _logger;
    private readonly string _keyPrefix;
    private readonly TimeSpan _connectionTtl = HubConstants.DefaultConnectionTrackingTtl;

    public RedisConnectionTracker(
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<RedisConnectionTracker> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyPrefix = configuration.GetValue<string>(HubConstants.RedisKeyPrefixConfigKey)
            ?? HubConstants.DefaultRedisKeyPrefix;
    }

    public async Task AddConnectionAsync(Guid showtimeId, string connectionId, string userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var showtimeKey = RedisHubKeyBuilder.ForShowtimeConnections(_keyPrefix, showtimeId);
            var connectionKey = RedisHubKeyBuilder.ForConnectionShowtimes(_keyPrefix, connectionId);

            // Add connection to showtime set
            await db.SetAddAsync(showtimeKey, connectionId);
            await db.KeyExpireAsync(showtimeKey, _connectionTtl);

            // Add showtime to connection's set (for cleanup)
            await db.SetAddAsync(connectionKey, showtimeId.ToString());
            await db.KeyExpireAsync(connectionKey, _connectionTtl);

            _logger.LogDebug(
                "Added connection {ConnectionId} (user {UserId}) to showtime {ShowtimeId}",
                connectionId, userId, showtimeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to add connection {ConnectionId} to showtime {ShowtimeId}",
                connectionId, showtimeId);
            throw;
        }
    }

    public async Task RemoveConnectionAsync(Guid showtimeId, string connectionId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var showtimeKey = RedisHubKeyBuilder.ForShowtimeConnections(_keyPrefix, showtimeId);
            var connectionKey = RedisHubKeyBuilder.ForConnectionShowtimes(_keyPrefix, connectionId);

            // Remove connection from showtime set
            await db.SetRemoveAsync(showtimeKey, connectionId);

            // Remove showtime from connection's set
            await db.SetRemoveAsync(connectionKey, showtimeId.ToString());

            _logger.LogDebug(
                "Removed connection {ConnectionId} from showtime {ShowtimeId}",
                connectionId, showtimeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to remove connection {ConnectionId} from showtime {ShowtimeId}",
                connectionId, showtimeId);
            // Don't throw - removal should be best-effort
        }
    }

    public async Task RemoveConnectionFromAllShowtimesAsync(string connectionId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var connectionKey = RedisHubKeyBuilder.ForConnectionShowtimes(_keyPrefix, connectionId);

            // Get all showtimes this connection is in
            var showtimeIds = await db.SetMembersAsync(connectionKey);

            if (showtimeIds.Length == 0)
            {
                _logger.LogDebug(
                    "Connection {ConnectionId} was not in any showtimes",
                    connectionId);
                return;
            }

            // Remove connection from all showtime sets
            foreach (var showtimeIdValue in showtimeIds)
            {
                if (Guid.TryParse(showtimeIdValue, out var showtimeId))
                {
                    var showtimeKey = RedisHubKeyBuilder.ForShowtimeConnections(_keyPrefix, showtimeId);
                    await db.SetRemoveAsync(showtimeKey, connectionId);
                }
            }

            // Delete connection's showtime set
            await db.KeyDeleteAsync(connectionKey);

            _logger.LogInformation(
                "Removed connection {ConnectionId} from {Count} showtimes",
                connectionId, showtimeIds.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to remove connection {ConnectionId} from all showtimes",
                connectionId);
            // Don't throw - cleanup should be best-effort
        }
    }

    public async Task<int> GetViewerCountAsync(Guid showtimeId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var showtimeKey = RedisHubKeyBuilder.ForShowtimeConnections(_keyPrefix, showtimeId);

            var count = await db.SetLengthAsync(showtimeKey);
            return (int)count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get viewer count for showtime {ShowtimeId}",
                showtimeId);
            return 0; // Return 0 on error
        }
    }

    public async Task<List<Guid>> GetShowtimesForConnectionAsync(string connectionId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var connectionKey = RedisHubKeyBuilder.ForConnectionShowtimes(_keyPrefix, connectionId);

            var showtimeIds = await db.SetMembersAsync(connectionKey);

            var result = new List<Guid>();
            foreach (var showtimeIdValue in showtimeIds)
            {
                if (Guid.TryParse(showtimeIdValue, out var showtimeId))
                {
                    result.Add(showtimeId);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get showtimes for connection {ConnectionId}",
                connectionId);
            return new List<Guid>();
        }
    }
}
