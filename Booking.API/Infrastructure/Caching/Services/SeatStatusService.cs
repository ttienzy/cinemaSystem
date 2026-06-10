using Booking.API.Infrastructure.Caching.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Redis-only implementation of seat status management.
/// Upstream application services own the full availability response and source seat layout from API/DB.
/// </summary>
public class SeatStatusService : ISeatStatusService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SeatStatusService> _logger;
    private readonly TimeSpan _lockDuration;
    private readonly TimeSpan _seatMapExpiration;
    private readonly string _keyPrefix;

    public SeatStatusService(
        IConnectionMultiplexer redis,
        ILogger<SeatStatusService> logger,
        IConfiguration configuration)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var lockMinutes = configuration.GetValue<int>("SeatLock:LockDurationMinutes", 10);
        var expirationHours = configuration.GetValue<int>("Redis:SeatMapExpirationHours", 24);

        _lockDuration = TimeSpan.FromMinutes(lockMinutes);
        _seatMapExpiration = TimeSpan.FromHours(expirationHours);
        _keyPrefix = configuration.GetValue<string>("Redis:KeyPrefix") ?? "cinema";
    }

    public async Task<List<SeatStatusDto>> GetCachedSeatStatusesAsync(Guid showtimeId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        if (!await db.KeyExistsAsync(seatMapKey))
        {
            return [];
        }

        var entries = await db.HashGetAllAsync(seatMapKey);
        var seats = new List<SeatStatusDto>(entries.Length);

        foreach (var entry in entries)
        {
            var seatData = DeserializeSeat(entry.Value);
            if (seatData is null)
            {
                continue;
            }

            if (seatData.IsLockExpired())
            {
                seatData.ReleaseLock();
                await SaveSeatAsync(db, seatMapKey, entry.Name!, seatData);
            }

            seats.Add(ToSeatStatusDto(seatData));
        }

        seats = seats
            .OrderBy(seat => seat.Row, StringComparer.OrdinalIgnoreCase)
            .ThenBy(seat => seat.Number)
            .ToList();

        return seats;
    }

    public async Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId)
    {
        var db = _redis.GetDatabase();
        var metadata = await GetMetadataAsync(db, showtimeId) ?? new SeatMapMetadata
        {
            ShowtimeId = showtimeId,
            CinemaHallId = cinemaHallId
        };

        metadata.CinemaHallId = cinemaHallId;
        await SaveMetadataAsync(db, metadata);
        await RefreshExpirationAsync(db, showtimeId);
    }

    public async Task InitializeSeatMapAsync(
        Guid showtimeId,
        Guid cinemaHallId,
        string cinemaHallName,
        IReadOnlyCollection<SeatStatusDto> seats)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        var hashEntries = seats
            .Select(seat => new HashEntry(
                GetSeatFieldKey(seat.SeatId),
                JsonSerializer.Serialize(new RedisSeatData
                {
                    SeatId = seat.SeatId,
                    Row = seat.Row,
                    Number = seat.Number,
                    Price = seat.Price,
                    Status = seat.Status,
                    UserId = seat.LockedBy,
                    LockedUntil = seat.LockedUntil
                })))
            .ToArray();

        if (hashEntries.Length > 0)
        {
            await db.HashSetAsync(seatMapKey, hashEntries);
        }

        await SaveMetadataAsync(db, new SeatMapMetadata
        {
            ShowtimeId = showtimeId,
            CinemaHallId = cinemaHallId,
            CinemaHallName = cinemaHallName
        });

        await RefreshExpirationAsync(db, showtimeId);

        _logger.LogInformation(
            "Initialized Redis seat map for showtime {ShowtimeId} with {Count} seats",
            showtimeId,
            seats.Count);
    }

    public async Task<SeatLockResult> LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        if (!await db.KeyExistsAsync(seatMapKey))
        {
            return new SeatLockResult
            {
                Success = false,
                Message = "Seat map not initialized",
                AlreadyLockedSeats = seatIds
            };
        }

        var lockedUntil = DateTime.UtcNow.Add(_lockDuration);
        var script = @"
            local seatMapKey = KEYS[1]
            local userId = ARGV[1]
            local lockedUntil = ARGV[2]
            local now = ARGV[3]
            local lockedSeats = {}
            local failedSeats = {}

            for i = 4, #ARGV do
                local seatKey = ARGV[i]
                local seatDataJson = redis.call('HGET', seatMapKey, seatKey)

                if seatDataJson then
                    local seatData = cjson.decode(seatDataJson)

                    if seatData.Status == 0 or
                       (seatData.Status == 1 and seatData.LockedUntil and seatData.LockedUntil < now) or
                       (seatData.Status == 1 and seatData.UserId == userId) then
                        seatData.Status = 1
                        seatData.UserId = userId
                        seatData.LockedAt = now
                        seatData.LockedUntil = lockedUntil
                        seatData.BookingId = cjson.null
                        seatData.BookedAt = cjson.null

                        redis.call('HSET', seatMapKey, seatKey, cjson.encode(seatData))
                        table.insert(lockedSeats, seatKey)
                    else
                        table.insert(failedSeats, seatKey)
                    end
                else
                    table.insert(failedSeats, seatKey)
                end
            end

            local result = {}
            table.insert(result, #lockedSeats)

            for _, seat in ipairs(lockedSeats) do
                table.insert(result, seat)
            end

            for _, seat in ipairs(failedSeats) do
                table.insert(result, seat)
            end

            return result
        ";

        try
        {
            var values = new RedisValue[]
            {
                userId,
                lockedUntil.ToString("O"),
                DateTime.UtcNow.ToString("O")
            }.Concat(seatIds.Select(id => (RedisValue)GetSeatFieldKey(id))).ToArray();

            var resultArray = (RedisValue[])(await db.ScriptEvaluateAsync(
                script,
                [seatMapKey],
                values))!;

            var lockedCount = resultArray.Length > 0 ? (int)resultArray[0] : 0;
            var lockedSeats = resultArray
                .Skip(1)
                .Take(lockedCount)
                .Where(value => !value.IsNullOrEmpty)
                .Select(value => ExtractSeatIdFromKey((string)value!))
                .ToList();
            var failedSeats = resultArray
                .Skip(1 + lockedCount)
                .Where(value => !value.IsNullOrEmpty)
                .Select(value => ExtractSeatIdFromKey((string)value!))
                .ToList();

            return new SeatLockResult
            {
                Success = failedSeats.Count == 0,
                Message = failedSeats.Count == 0
                    ? $"Successfully locked {lockedSeats.Count} seat(s)"
                    : $"{failedSeats.Count} seat(s) are unavailable",
                LockedSeats = lockedSeats,
                AlreadyLockedSeats = failedSeats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking seats for user {UserId}", userId);
            return new SeatLockResult
            {
                Success = false,
                Message = "Failed to lock seats due to Redis error"
            };
        }
    }

    public async Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var allUnlocked = true;

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null)
            {
                allUnlocked = false;
                continue;
            }

            if (seatData.Status != SeatStatus.Locked || !seatData.IsOwnedBy(userId))
            {
                allUnlocked = false;
                continue;
            }

            seatData.ReleaseLock();
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return allUnlocked;
    }

    public async Task<bool> MarkSeatsAsBookedAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var allBooked = true;

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null)
            {
                allBooked = false;
                continue;
            }

            seatData.MarkBooked(bookingId);
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return allBooked;
    }

    public async Task<bool> ReleaseBookedSeatsAsync(Guid showtimeId, List<Guid> seatIds)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var allReleased = true;

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null || seatData.Status != SeatStatus.Booked)
            {
                allReleased = false;
                continue;
            }

            seatData.ReleaseBooking();
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return allReleased;
    }

    public async Task<bool> ReleaseSeatsForBookingAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var allReleased = true;

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null || seatData.BookingId != bookingId)
            {
                allReleased = false;
                continue;
            }

            seatData.ReleaseBooking();
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return allReleased;
    }

    public async Task<bool> AreSeatsAvailableAsync(Guid showtimeId, List<Guid> seatIds)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null)
            {
                return false;
            }

            if (seatData.IsLockExpired())
            {
                seatData.ReleaseLock();
                await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
            }

            if (seatData.Status != SeatStatus.Available)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<SeatStatusInfo> GetSeatStatusAsync(Guid showtimeId, Guid seatId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var seatKey = GetSeatFieldKey(seatId);
        var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

        if (seatData is null)
        {
            return new SeatStatusInfo
            {
                SeatId = seatId,
                Status = SeatStatus.Unavailable
            };
        }

        if (seatData.IsLockExpired())
        {
            seatData.ReleaseLock();
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return new SeatStatusInfo
        {
            SeatId = seatId,
            Status = seatData.Status,
            UserId = seatData.UserId,
            BookingId = seatData.BookingId,
            LockedUntil = seatData.LockedUntil
        };
    }

    public async Task<bool> ExtendSeatLocksAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var newLockedUntil = DateTime.UtcNow.Add(_lockDuration);
        var allExtended = true;

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatData = DeserializeSeat(await db.HashGetAsync(seatMapKey, seatKey));

            if (seatData is null ||
                seatData.Status != SeatStatus.Locked ||
                !seatData.IsOwnedBy(userId) ||
                seatData.IsLockExpired())
            {
                allExtended = false;
                continue;
            }

            seatData.LockedUntil = newLockedUntil;
            await SaveSeatAsync(db, seatMapKey, seatKey, seatData);
        }

        return allExtended;
    }

    public async Task CleanupExpiredLocksAsync(Guid showtimeId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var allSeats = await db.HashGetAllAsync(seatMapKey);
        var cleanedCount = 0;

        foreach (var entry in allSeats)
        {
            var seatData = DeserializeSeat(entry.Value);
            if (seatData is null || !seatData.IsLockExpired())
            {
                continue;
            }

            seatData.ReleaseLock();
            await SaveSeatAsync(db, seatMapKey, entry.Name!, seatData);
            cleanedCount++;
        }

        _logger.LogInformation(
            "Cleaned up {Count} expired seat locks for showtime {ShowtimeId}",
            cleanedCount,
            showtimeId);
    }

    public async Task<SeatBookingResult> VerifyAndMarkAsBookedAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string userId,
        Guid bookingId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        if (!await db.KeyExistsAsync(seatMapKey))
        {
            return new SeatBookingResult
            {
                Success = false,
                Message = "Seat map not initialized",
                FailureReason = SeatBookingFailureReason.Unavailable,
                FailedSeats = seatIds
            };
        }

        var now = DateTime.UtcNow;
        var script = @"
            local seatMapKey = KEYS[1]
            local userId = ARGV[1]
            local bookingId = ARGV[2]
            local now = ARGV[3]
            local bookedSeats = {}
            local failedSeats = {}
            local failureReason = 0

            for i = 4, #ARGV do
                local seatKey = ARGV[i]
                local seatDataJson = redis.call('HGET', seatMapKey, seatKey)

                if not seatDataJson then
                    table.insert(failedSeats, seatKey)
                    if failureReason == 0 then failureReason = 5 end
                else
                    local seatData = cjson.decode(seatDataJson)

                    if seatData.Status == 2 then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 4 end
                    elseif seatData.Status ~= 1 then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 1 end
                    elseif seatData.LockedUntil and seatData.LockedUntil < now then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 2 end
                    elseif seatData.UserId ~= userId then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 3 end
                    else
                        seatData.Status = 2
                        seatData.BookingId = bookingId
                        seatData.BookedAt = now
                        seatData.LockedAt = cjson.null
                        seatData.LockedUntil = cjson.null

                        redis.call('HSET', seatMapKey, seatKey, cjson.encode(seatData))
                        table.insert(bookedSeats, seatKey)
                    end
                end
            end

            local result = {}
            table.insert(result, #bookedSeats)
            table.insert(result, failureReason)

            for _, seat in ipairs(bookedSeats) do
                table.insert(result, seat)
            end

            for _, seat in ipairs(failedSeats) do
                table.insert(result, seat)
            end

            return result
        ";

        try
        {
            var values = new RedisValue[]
            {
                userId,
                bookingId.ToString(),
                now.ToString("O")
            }.Concat(seatIds.Select(id => (RedisValue)GetSeatFieldKey(id))).ToArray();

            var resultArray = (RedisValue[])(await db.ScriptEvaluateAsync(
                script,
                [seatMapKey],
                values))!;

            if (resultArray.Length < 2)
            {
                return new SeatBookingResult
                {
                    Success = false,
                    Message = "Unexpected Redis booking result",
                    FailureReason = SeatBookingFailureReason.Unavailable
                };
            }

            var bookedCount = (int)resultArray[0];
            var failureReason = (SeatBookingFailureReason)(int)resultArray[1];
            var bookedSeats = resultArray
                .Skip(2)
                .Take(bookedCount)
                .Where(value => !value.IsNullOrEmpty)
                .Select(value => ExtractSeatIdFromKey((string)value!))
                .ToList();
            var failedSeats = resultArray
                .Skip(2 + bookedCount)
                .Where(value => !value.IsNullOrEmpty)
                .Select(value => ExtractSeatIdFromKey((string)value!))
                .ToList();

            return new SeatBookingResult
            {
                Success = failedSeats.Count == 0,
                Message = failedSeats.Count == 0
                    ? $"Successfully booked {bookedSeats.Count} seat(s)"
                    : GetBookingFailureMessage(failureReason, failedSeats.Count),
                BookedSeats = bookedSeats,
                FailedSeats = failedSeats,
                FailureReason = failedSeats.Count == 0 ? SeatBookingFailureReason.None : failureReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking seats for user {UserId}", userId);
            return new SeatBookingResult
            {
                Success = false,
                Message = "Failed to book seats due to Redis error",
                FailureReason = SeatBookingFailureReason.Unavailable
            };
        }
    }

    private string GetSeatMapKey(Guid showtimeId) => $"{_keyPrefix}:showtime:{showtimeId}:seats";
    private string GetSeatMetadataKey(Guid showtimeId) => $"{_keyPrefix}:showtime:{showtimeId}:metadata";
    private static string GetSeatFieldKey(Guid seatId) => $"seat:{seatId}";
    private static Guid ExtractSeatIdFromKey(string key) => Guid.Parse(key.Replace("seat:", ""));

    private async Task<SeatMapMetadata?> GetMetadataAsync(IDatabase db, Guid showtimeId)
    {
        var value = await db.StringGetAsync(GetSeatMetadataKey(showtimeId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<SeatMapMetadata>((string)value!);
    }

    private async Task SaveMetadataAsync(IDatabase db, SeatMapMetadata metadata)
    {
        await db.StringSetAsync(
            GetSeatMetadataKey(metadata.ShowtimeId),
            JsonSerializer.Serialize(metadata),
            _seatMapExpiration);
    }

    private async Task RefreshExpirationAsync(IDatabase db, Guid showtimeId)
    {
        await db.KeyExpireAsync(GetSeatMapKey(showtimeId), _seatMapExpiration);
        await db.KeyExpireAsync(GetSeatMetadataKey(showtimeId), _seatMapExpiration);
    }

    private static RedisSeatData? DeserializeSeat(RedisValue value)
        => value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<RedisSeatData>((string)value!);

    private static async Task SaveSeatAsync(IDatabase db, RedisKey seatMapKey, RedisValue seatKey, RedisSeatData seatData)
    {
        await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
    }

    private static SeatStatusDto ToSeatStatusDto(RedisSeatData seatData)
        => new()
        {
            SeatId = seatData.SeatId,
            Row = seatData.Row,
            Number = seatData.Number,
            Status = seatData.Status,
            Price = seatData.Price,
            LockedBy = seatData.UserId,
            LockedUntil = seatData.LockedUntil
        };

    private static string GetBookingFailureMessage(SeatBookingFailureReason reason, int failedCount)
        => reason switch
        {
            SeatBookingFailureReason.NotLocked =>
                $"{failedCount} seat(s) are not locked. Please lock seats before booking.",
            SeatBookingFailureReason.LockExpired =>
                $"{failedCount} seat(s) lock has expired. Please select seats again.",
            SeatBookingFailureReason.WrongUser =>
                $"{failedCount} seat(s) are locked by another user.",
            SeatBookingFailureReason.AlreadyBooked =>
                $"{failedCount} seat(s) are already booked.",
            _ => $"{failedCount} seat(s) cannot be booked."
        };
}
