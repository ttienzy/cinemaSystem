using Application.Common.Interfaces.Services;
using StackExchange.Redis;

namespace Infrastructure.Redis
{
    public class SeatLockService(IConnectionMultiplexer connectionMultiplexer) : ISeatLockService
    {
        private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

        private static string GetKey(Guid showtimeId, Guid seatId) => $"cinema:showtime:{showtimeId}:seat:{seatId}:lock";

        public async Task LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId, TimeSpan ttl, CancellationToken ct = default)
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var seatId in seatIds)
            {
                var key = GetKey(showtimeId, seatId);
                // NX (When.NotExists) ensures we only set it if not already locked
                tasks.Add(batch.StringSetAsync(key, bookingId.ToString(), ttl, When.NotExists));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task ReleaseSeatsAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default)
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var seatId in seatIds)
            {
                tasks.Add(batch.KeyDeleteAsync(GetKey(showtimeId, seatId)));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task<Dictionary<Guid, SeatLockStatus>> GetSeatStatusesAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default)
        {
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
