using Booking.API.Application.DTOs.External;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Infrastructure.Caching.Models;
using Booking.API.Infrastructure.Hubs.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Redis-based implementation of seat status management
/// Uses Redis Hash for atomic operations and efficient storage
/// </summary>
public class SeatStatusService : ISeatStatusService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IExternalServiceClient _externalClient;
    private readonly ILogger<SeatStatusService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISeatNotificationService _notificationService;
    private readonly TimeSpan _lockDuration;

    // Redis key patterns (configurable prefix)
    private readonly string _keyPrefix = "cinema"; // Default value

    public SeatStatusService(
        IConnectionMultiplexer redis,
        IExternalServiceClient externalClient,
        ILogger<SeatStatusService> logger,
        IConfiguration configuration,
        ISeatNotificationService notificationService)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Get configuration values (no hardcoding!)
        var lockMinutes = _configuration.GetValue<int>("SeatLock:LockDurationMinutes", 10);
        _lockDuration = TimeSpan.FromMinutes(lockMinutes);
        _keyPrefix = _configuration.GetValue<string>("Redis:KeyPrefix") ?? "cinema";

        _logger.LogInformation("SeatStatusService initialized with lock duration: {Duration} minutes", lockMinutes);
    }

    public async Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(Guid showtimeId)
    {
        _logger.LogInformation("Getting seat availability for showtime {ShowtimeId}", showtimeId);

        // 1. Get showtime info from Movie.API
        var showtime = await _externalClient.GetShowtimeByIdAsync(showtimeId);
        if (showtime == null)
        {
            throw new InvalidOperationException($"Showtime {showtimeId} not found");
        }

        // 2. Get cinema hall info
        var cinemaHall = await _externalClient.GetCinemaHallByIdAsync(showtime.CinemaHallId);
        if (cinemaHall == null)
        {
            throw new InvalidOperationException($"Cinema hall {showtime.CinemaHallId} not found");
        }

        // 3. Get physical seats from Cinema.API
        var seats = await _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);
        if (!seats.Any())
        {
            _logger.LogWarning("No seats found for cinema hall {HallId}", showtime.CinemaHallId);
            return new SeatAvailabilityResponse
            {
                ShowtimeId = showtimeId,
                CinemaHallId = showtime.CinemaHallId,
                CinemaHallName = cinemaHall.Name,
                Seats = new List<SeatStatusDto>(),
                Summary = new SeatAvailabilitySummary()
            };
        }

        // 4. Initialize seat map in Redis if not exists
        await InitializeSeatMapAsync(showtimeId, showtime.CinemaHallId);

        // 5. Get seat status from Redis
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        var seatStatuses = new List<SeatStatusDto>();
        var summary = new SeatAvailabilitySummary { TotalSeats = seats.Count };

        foreach (var seat in seats)
        {
            var seatKey = GetSeatFieldKey(seat.Id);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            SeatStatus status = SeatStatus.Available;
            string? lockedBy = null;
            DateTime? lockedUntil = null;

            if (!seatDataJson.IsNullOrEmpty)
            {
                var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
                if (seatData != null)
                {
                    // Check if lock expired
                    if (seatData.IsLockExpired())
                    {
                        // Auto-release expired lock
                        await UnlockSeatInternalAsync(db, showtimeId, seat.Id);
                        status = SeatStatus.Available;
                    }
                    else
                    {
                        status = seatData.Status;
                        lockedBy = seatData.UserId;
                        lockedUntil = seatData.LockedUntil;
                    }
                }
            }

            seatStatuses.Add(new SeatStatusDto
            {
                SeatId = seat.Id,
                Row = seat.Row,
                Number = seat.Number,
                Status = status,
                Price = showtime.Price,
                LockedBy = lockedBy,
                LockedUntil = lockedUntil
            });

            // Update summarys
            switch (status)
            {
                case SeatStatus.Available:
                    summary.AvailableSeats++;
                    break;
                case SeatStatus.Locked:
                    summary.LockedSeats++;
                    break;
                case SeatStatus.Booked:
                    summary.BookedSeats++;
                    break;
            }
        }

        return new SeatAvailabilityResponse
        {
            ShowtimeId = showtimeId,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHall.Name,
            Seats = seatStatuses,
            Summary = summary
        };
    }

    public async Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        // Check if already initialized
        var exists = await db.KeyExistsAsync(seatMapKey);
        if (exists)
        {
            _logger.LogDebug("Seat map for showtime {ShowtimeId} already initialized", showtimeId);
            return;
        }

        _logger.LogInformation("Initializing seat map for showtime {ShowtimeId}", showtimeId);

        // Get seats from Cinema.API
        var seats = await _externalClient.GetSeatsByCinemaHallIdAsync(cinemaHallId);
        if (!seats.Any())
        {
            _logger.LogWarning("No seats to initialize for showtime {ShowtimeId}", showtimeId);
            return;
        }

        // Initialize all seats as Available
        var hashEntries = seats.Select(seat =>
        {
            var seatData = new RedisSeatData
            {
                Status = SeatStatus.Available,
                UserId = null,
                BookingId = null,
                LockedAt = null,
                LockedUntil = null,
                BookedAt = null
            };

            return new HashEntry(
                GetSeatFieldKey(seat.Id),
                JsonSerializer.Serialize(seatData)
            );
        }).ToArray();

        await db.HashSetAsync(seatMapKey, hashEntries);

        // Set expiration (showtime end time + buffer)
        var expirationHours = _configuration.GetValue<int>("Redis:SeatMapExpirationHours", 24);
        await db.KeyExpireAsync(seatMapKey, TimeSpan.FromHours(expirationHours));

        _logger.LogInformation("Initialized {Count} seats for showtime {ShowtimeId}", seats.Count, showtimeId);
    }

    public async Task<SeatLockResult> LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        _logger.LogInformation("Attempting to lock {Count} seats for user {UserId} in showtime {ShowtimeId}",
            seatIds.Count, userId, showtimeId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        // Ensure seat map exists
        if (!await db.KeyExistsAsync(seatMapKey))
        {
            throw new InvalidOperationException($"Seat map not initialized for showtime {showtimeId}");
        }

        var result = new SeatLockResult { Success = true };
        var lockedUntil = DateTime.UtcNow.Add(_lockDuration);

        // Use Lua script for atomic multi-seat locking
        // Return flattened array: [lockedCount, ...lockedSeats, ...alreadyLocked]
        // This avoids nested array casting issues
        var script = @"
            local seatMapKey = KEYS[1]
            local userId = ARGV[1]
            local lockedUntil = ARGV[2]
            local now = ARGV[3]

            local lockedSeats = {}
            local alreadyLocked = {}

            for i = 4, #ARGV do
                local seatKey = ARGV[i]
                local seatDataJson = redis.call('HGET', seatMapKey, seatKey)

                if seatDataJson then
                    local seatData = cjson.decode(seatDataJson)

                    -- Check if available or lock expired or owned by same user
                    if seatData.Status == 0 or
                       (seatData.Status == 1 and seatData.LockedUntil and seatData.LockedUntil < now) or
                       (seatData.Status == 1 and seatData.UserId == userId) then

                        -- Lock the seat
                        seatData.Status = 1
                        seatData.UserId = userId
                        seatData.LockedAt = now
                        seatData.LockedUntil = lockedUntil

                        redis.call('HSET', seatMapKey, seatKey, cjson.encode(seatData))
                        table.insert(lockedSeats, seatKey)
                    else
                        table.insert(alreadyLocked, seatKey)
                    end
                else
                    -- Seat not found in seat map — treat as failure
                    table.insert(alreadyLocked, seatKey)
                end
            end

            -- Return flattened array: [lockedCount, ...lockedSeats, ...alreadyLocked]
            -- This avoids nested array casting issues in C#
            local result = {}
            table.insert(result, #lockedSeats)  -- First element is count

            for _, seat in ipairs(lockedSeats) do
                table.insert(result, seat)
            end

            for _, seat in ipairs(alreadyLocked) do
                table.insert(result, seat)
            end

            return result
        ";

        try
        {
            var keys = new RedisKey[] { seatMapKey };
            var values = new RedisValue[]
            {
                userId,
                lockedUntil.ToString("O"),
                DateTime.UtcNow.ToString("O")
            }.Concat(seatIds.Select(id => (RedisValue)GetSeatFieldKey(id))).ToArray();

            var scriptResult = await db.ScriptEvaluateAsync(script, keys, values);

            // Parse flattened array: [lockedCount, ...lockedSeats, ...alreadyLocked]
            var resultArray = (RedisValue[])scriptResult!;

            if (resultArray.Length == 0)
            {
                _logger.LogWarning("Script returned empty array");
                result.Success = false;
                result.Message = "Failed to lock seats - script returned no data";
                return result;
            }

            // First element is the count of locked seats
            var lockedCount = (int)resultArray[0];

            // Extract locked seats (from index 1 to lockedCount)
            var lockedSeatKeys = resultArray
                .Skip(1)
                .Take(lockedCount)
                .Where(rv => !rv.IsNullOrEmpty)
                .Select(rv => (string)rv!)
                .ToList();

            // Extract already locked seats (remaining elements after locked seats)
            var alreadyLockedKeys = resultArray
                .Skip(1 + lockedCount)
                .Where(rv => !rv.IsNullOrEmpty)
                .Select(rv => (string)rv!)
                .ToList();

            _logger.LogDebug("Lua script returned - lockedSeats: {LockedCount}, alreadyLocked: {AlreadyLockedCount}",
                lockedSeatKeys.Count, alreadyLockedKeys.Count);

            result.LockedSeats = lockedSeatKeys.Select(ExtractSeatIdFromKey).ToList();
            result.AlreadyLockedSeats = alreadyLockedKeys.Select(ExtractSeatIdFromKey).ToList();

            if (result.AlreadyLockedSeats.Any())
            {
                result.Success = false;
                result.Message = $"{result.AlreadyLockedSeats.Count} seat(s) are already locked or booked";
                _logger.LogWarning("Failed to lock some seats for user {UserId}: {Count} already locked",
                    userId, result.AlreadyLockedSeats.Count);
            }
            else
            {
                result.Message = $"Successfully locked {result.LockedSeats.Count} seat(s)";
                _logger.LogInformation("Successfully locked {Count} seats for user {UserId}",
                    result.LockedSeats.Count, userId);

                // Broadcast seat locked notification to all clients watching this showtime
                await _notificationService.NotifySeatLockedAsync(
                    showtimeId,
                    result.LockedSeats,
                    userId,
                    lockedUntil);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking seats for user {UserId}", userId);
            result.Success = false;
            result.Message = "Failed to lock seats due to system error";
        }

        return result;
    }

    public async Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        _logger.LogInformation("Unlocking {Count} seats for user {UserId} in showtime {ShowtimeId}",
            seatIds.Count, userId, showtimeId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        var allUnlocked = true;
        var unlockedSeats = new List<Guid>();

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            if (seatDataJson.IsNullOrEmpty)
                continue;

            var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
            if (seatData == null)
                continue;

            // Only unlock if owned by this user and status is Locked
            if (seatData.Status == SeatStatus.Locked && seatData.IsOwnedBy(userId))
            {
                seatData.Status = SeatStatus.Available;
                seatData.UserId = null;
                seatData.LockedAt = null;
                seatData.LockedUntil = null;

                await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
                unlockedSeats.Add(seatId);
            }
            else
            {
                _logger.LogWarning("Cannot unlock seat {SeatId} - not owned by user {UserId} or not locked",
                    seatId, userId);
                allUnlocked = false;
            }
        }

        // Broadcast seat unlocked notification if any seats were unlocked
        if (unlockedSeats.Any())
        {
            await _notificationService.NotifySeatUnlockedAsync(showtimeId, unlockedSeats);
        }

        return allUnlocked;
    }

    public async Task<bool> MarkSeatsAsBookedAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId)
    {
        _logger.LogInformation("Marking {Count} seats as booked for booking {BookingId}",
            seatIds.Count, bookingId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            if (seatDataJson.IsNullOrEmpty)
                continue;

            var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
            if (seatData == null)
                continue;

            seatData.Status = SeatStatus.Booked;
            seatData.BookingId = bookingId;
            seatData.BookedAt = DateTime.UtcNow;
            seatData.LockedAt = null;
            seatData.LockedUntil = null;

            await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
        }

        _logger.LogInformation("Marked {Count} seats as booked", seatIds.Count);

        // Broadcast seat booked notification
        await _notificationService.NotifySeatBookedAsync(showtimeId, seatIds);

        return true;
    }

    public async Task<bool> ReleaseBookedSeatsAsync(Guid showtimeId, List<Guid> seatIds)
    {
        _logger.LogInformation("Releasing {Count} booked seats for showtime {ShowtimeId}",
            seatIds.Count, showtimeId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            if (seatDataJson.IsNullOrEmpty)
                continue;

            var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
            if (seatData == null)
                continue;

            seatData.Status = SeatStatus.Available;
            seatData.UserId = null;
            seatData.BookingId = null;
            seatData.BookedAt = null;

            await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
        }

        _logger.LogInformation("Released {Count} seats", seatIds.Count);

        // Broadcast seat released notification
        await _notificationService.NotifySeatReleasedAsync(showtimeId, seatIds);
        return true;
    }

    public async Task<bool> AreSeatsAvailableAsync(Guid showtimeId, List<Guid> seatIds)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            if (seatDataJson.IsNullOrEmpty)
                return false;

            var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
            if (seatData == null)
                return false;

            // Check if locked and expired
            if (seatData.IsLockExpired())
            {
                await UnlockSeatInternalAsync(db, showtimeId, seatId);
                continue;
            }

            if (seatData.Status != SeatStatus.Available)
                return false;
        }

        return true;
    }

    public async Task<SeatStatusInfo> GetSeatStatusAsync(Guid showtimeId, Guid seatId)
    {
        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var seatKey = GetSeatFieldKey(seatId);

        var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

        if (seatDataJson.IsNullOrEmpty)
        {
            return new SeatStatusInfo
            {
                SeatId = seatId,
                Status = SeatStatus.Unavailable
            };
        }

        var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
        if (seatData == null)
        {
            return new SeatStatusInfo
            {
                SeatId = seatId,
                Status = SeatStatus.Unavailable
            };
        }

        // Auto-release expired lock
        if (seatData.IsLockExpired())
        {
            await UnlockSeatInternalAsync(db, showtimeId, seatId);
            return new SeatStatusInfo
            {
                SeatId = seatId,
                Status = SeatStatus.Available
            };
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
        _logger.LogInformation("Extending locks for {Count} seats for user {UserId}",
            seatIds.Count, userId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);
        var newLockedUntil = DateTime.UtcNow.Add(_lockDuration);

        foreach (var seatId in seatIds)
        {
            var seatKey = GetSeatFieldKey(seatId);
            var seatDataJson = await db.HashGetAsync(seatMapKey, seatKey);

            if (seatDataJson.IsNullOrEmpty)
                continue;

            var seatData = JsonSerializer.Deserialize<RedisSeatData>(seatDataJson!);
            if (seatData == null || !seatData.IsOwnedBy(userId) || seatData.Status != SeatStatus.Locked)
                continue;

            seatData.LockedUntil = newLockedUntil;
            await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
        }

        return true;
    }

    public async Task CleanupExpiredLocksAsync(Guid showtimeId)
    {
        _logger.LogInformation("Cleaning up expired locks for showtime {ShowtimeId}", showtimeId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        var allSeats = await db.HashGetAllAsync(seatMapKey);
        var now = DateTime.UtcNow;
        var cleanedCount = 0;

        foreach (var entry in allSeats)
        {
            var seatData = JsonSerializer.Deserialize<RedisSeatData>(entry.Value!);
            if (seatData != null && seatData.IsLockExpired())
            {
                seatData.Status = SeatStatus.Available;
                seatData.UserId = null;
                seatData.LockedAt = null;
                seatData.LockedUntil = null;

                await db.HashSetAsync(seatMapKey, entry.Name, JsonSerializer.Serialize(seatData));
                cleanedCount++;
            }
        }

        _logger.LogInformation("Cleaned up {Count} expired locks for showtime {ShowtimeId}",
            cleanedCount, showtimeId);
    }

    public async Task<SeatBookingResult> VerifyAndMarkAsBookedAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string userId,
        Guid bookingId)
    {
        _logger.LogInformation(
            "Verifying and marking {Count} seats as booked for user {UserId}, booking {BookingId}",
            seatIds.Count, userId, bookingId);

        var db = _redis.GetDatabase();
        var seatMapKey = GetSeatMapKey(showtimeId);

        // Ensure seat map exists
        if (!await db.KeyExistsAsync(seatMapKey))
        {
            _logger.LogWarning("Seat map not found for showtime {ShowtimeId}", showtimeId);
            return new SeatBookingResult
            {
                Success = false,
                Message = "Seat map not initialized",
                FailureReason = SeatBookingFailureReason.Unavailable
            };
        }

        var result = new SeatBookingResult { Success = true };
        var now = DateTime.UtcNow;

        // Use Lua script for atomic verification and booking
        // Return flattened array: [bookedCount, failureReason, ...bookedSeats, ...failedSeats]
        var script = @"
            local seatMapKey = KEYS[1]
            local userId = ARGV[1]
            local bookingId = ARGV[2]
            local now = ARGV[3]
            
            local bookedSeats = {}
            local failedSeats = {}
            local failureReason = 0  -- 0=success, 1=NotLocked, 2=LockExpired, 3=WrongUser, 4=AlreadyBooked
            
            for i = 4, #ARGV do
                local seatKey = ARGV[i]
                local seatDataJson = redis.call('HGET', seatMapKey, seatKey)
                
                if not seatDataJson then
                    -- Seat doesn't exist
                    table.insert(failedSeats, seatKey)
                    if failureReason == 0 then failureReason = 1 end  -- NotLocked
                else
                    local seatData = cjson.decode(seatDataJson)
                    
                    -- Check 1: Is seat already booked?
                    if seatData.Status == 2 then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 4 end  -- AlreadyBooked
                    
                    -- Check 2: Is seat locked?
                    elseif seatData.Status ~= 1 then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 1 end  -- NotLocked
                    
                    -- Check 3: Is lock expired?
                    elseif seatData.LockedUntil and seatData.LockedUntil < now then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 2 end  -- LockExpired
                    
                    -- Check 4: Is locked by same user?
                    elseif seatData.UserId ~= userId then
                        table.insert(failedSeats, seatKey)
                        if failureReason == 0 then failureReason = 3 end  -- WrongUser
                    
                    -- All checks passed - mark as booked
                    else
                        seatData.Status = 2  -- Booked
                        seatData.BookingId = bookingId
                        seatData.BookedAt = now
                        seatData.LockedAt = cjson.null
                        seatData.LockedUntil = cjson.null
                        
                        redis.call('HSET', seatMapKey, seatKey, cjson.encode(seatData))
                        table.insert(bookedSeats, seatKey)
                    end
                end
            end
            
            -- Return flattened array: [bookedCount, failureReason, ...bookedSeats, ...failedSeats]
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
            var keys = new RedisKey[] { seatMapKey };
            var values = new RedisValue[]
            {
                userId,
                bookingId.ToString(),
                now.ToString("O")
            }.Concat(seatIds.Select(id => (RedisValue)GetSeatFieldKey(id))).ToArray();

            var scriptResult = await db.ScriptEvaluateAsync(script, keys, values);

            // Parse flattened array: [bookedCount, failureReason, ...bookedSeats, ...failedSeats]
            var resultArray = (RedisValue[])scriptResult!;

            if (resultArray.Length < 2)
            {
                _logger.LogWarning("Script returned unexpected array length: {Length}", resultArray.Length);
                result.Success = false;
                result.Message = "Failed to verify seats - unexpected script result";
                result.FailureReason = SeatBookingFailureReason.Unavailable;
                return result;
            }

            var bookedCount = (int)resultArray[0];
            var failureReasonCode = (int)resultArray[1];

            // Extract booked seats
            var bookedSeatKeys = resultArray
                .Skip(2)
                .Take(bookedCount)
                .Where(rv => !rv.IsNullOrEmpty)
                .Select(rv => (string)rv!)
                .ToList();

            // Extract failed seats
            var failedSeatKeys = resultArray
                .Skip(2 + bookedCount)
                .Where(rv => !rv.IsNullOrEmpty)
                .Select(rv => (string)rv!)
                .ToList();

            result.BookedSeats = bookedSeatKeys.Select(ExtractSeatIdFromKey).ToList();
            result.FailedSeats = failedSeatKeys.Select(ExtractSeatIdFromKey).ToList();

            if (result.FailedSeats.Any())
            {
                result.Success = false;
                result.FailureReason = (SeatBookingFailureReason)failureReasonCode;

                result.Message = result.FailureReason switch
                {
                    SeatBookingFailureReason.NotLocked =>
                        $"{result.FailedSeats.Count} seat(s) are not locked. Please lock seats before booking.",
                    SeatBookingFailureReason.LockExpired =>
                        $"{result.FailedSeats.Count} seat(s) lock has expired. Please select seats again.",
                    SeatBookingFailureReason.WrongUser =>
                        $"{result.FailedSeats.Count} seat(s) are locked by another user.",
                    SeatBookingFailureReason.AlreadyBooked =>
                        $"{result.FailedSeats.Count} seat(s) are already booked.",
                    _ => $"{result.FailedSeats.Count} seat(s) cannot be booked."
                };

                _logger.LogWarning(
                    "Failed to book some seats for user {UserId}: {Reason} - {Count} failed",
                    userId, result.FailureReason, result.FailedSeats.Count);
            }
            else
            {
                result.Message = $"Successfully booked {result.BookedSeats.Count} seat(s)";
                _logger.LogInformation(
                    "Successfully booked {Count} seats for user {UserId}, booking {BookingId}",
                    result.BookedSeats.Count, userId, bookingId);

                // Broadcast seat booked notification
                await _notificationService.NotifySeatBookedAsync(showtimeId, result.BookedSeats);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying and booking seats for user {UserId}", userId);
            result.Success = false;
            result.Message = "Failed to book seats due to system error";
            result.FailureReason = SeatBookingFailureReason.Unavailable;
        }

        return result;
    }

    // Private helper methods
    private string GetSeatMapKey(Guid showtimeId) => $"{_keyPrefix}:showtime:{showtimeId}:seats";
    private string GetSeatFieldKey(Guid seatId) => $"seat:{seatId}";
    private Guid ExtractSeatIdFromKey(string key) => Guid.Parse(key.Replace("seat:", ""));

    private async Task UnlockSeatInternalAsync(IDatabase db, Guid showtimeId, Guid seatId)
    {
        var seatMapKey = GetSeatMapKey(showtimeId);
        var seatKey = GetSeatFieldKey(seatId);

        var seatData = new RedisSeatData
        {
            Status = SeatStatus.Available,
            UserId = null,
            LockedAt = null,
            LockedUntil = null
        };

        await db.HashSetAsync(seatMapKey, seatKey, JsonSerializer.Serialize(seatData));
    }
}


