using Application.Common.Interfaces.Services;
using StackExchange.Redis;

namespace Infrastructure.Redis
{
    public class SeatLockService(IConnectionMultiplexer connectionMultiplexer) : ISeatLockService
    {
        private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

        private static string GetKey(Guid showtimeId, Guid seatId) => $"cinema:showtime:{showtimeId}:seat:{seatId}:lock";

        // Lua script for atomic multi-seat locking - all or nothing
        private const string LockSeatsScript = @"
            local keys = {}
            for i = 1, #ARGV do
                table.insert(keys, KEYS[i])
            end
            
            local bookingId = ARGV[#ARGV]
            local ttl = tonumber(ARGV[#ARGV - 1])
            local numSeats = #ARGV - 2
            
            -- Try to lock all seats
            for i = 1, numSeats do
                local key = keys[i]
                local result = redis.call('SET', key, bookingId, 'NX', 'EX', ttl)
                if result == false then
                    -- Already locked, rollback any previously locked seats
                    for j = 1, i - 1 do
                        redis.call('DEL', keys[j])
                    end
                    return 0
                end
            end
            return 1
        ";

        public async Task LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId, TimeSpan ttl, CancellationToken ct = default)
        {
            if (!seatIds.Any()) return;

            var keys = seatIds.Select(id => (RedisKey)GetKey(showtimeId, id)).ToArray();
            var args = seatIds.Select(id => (RedisValue)bookingId.ToString())
                .Concat(new[] { (RedisValue)(int)ttl.TotalMinutes })
                .Concat(new[] { (RedisValue)seatIds.Count })
                .ToArray();

            var result = await _database.ScriptEvaluateAsync(
                LockSeatsScript,
                keys,
                args);

            if ((int)result == 0)
            {
                throw new InvalidOperationException("One or more seats are already locked.");
            }
        }

        public async Task ReleaseSeatsAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default)
        {
            if (!seatIds.Any()) return;

            var keys = seatIds.Select(id => (RedisKey)GetKey(showtimeId, id)).ToArray();
            await _database.KeyDeleteAsync(keys);
        }

        public async Task<Dictionary<Guid, SeatLockStatus>> GetSeatStatusesAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default)
        {
            if (!seatIds.Any())
                return new Dictionary<Guid, SeatLockStatus>();

            var keys = seatIds.Select(id => (RedisKey)GetKey(showtimeId, id)).ToArray();
            var values = await _database.StringGetAsync(keys);

            var result = new Dictionary<Guid, SeatLockStatus>();
            for (int i = 0; i < seatIds.Count; i++)
            {
                result[seatIds[i]] = values[i].HasValue ? SeatLockStatus.Locked : SeatLockStatus.Available;
            }

            return result;
        }
    }
}
