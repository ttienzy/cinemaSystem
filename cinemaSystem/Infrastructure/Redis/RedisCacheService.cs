using Application.Interfaces.Integrations;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace Infrastructure.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue || value.IsNull)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(value!);
        }
        public async Task<List<T>> GetAsync<T>(List<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();

            var redisValues = await _database.StringGetAsync(redisKeys);
            
            var result = new List<T>();

            foreach (var value in redisValues)
            {
                if (value.HasValue && !value.IsNull)
                {
                    var deserializedValue = JsonConvert.DeserializeObject<T>(value!);
                    if (deserializedValue != null)
                    {
                        result.Add(deserializedValue);
                    }
                }
            }
            return result;
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, serializedValue, expiration, keepTtl:true);
        }
        public async Task UpdateAsync<T>(string key, T value)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, serializedValue, keepTtl: true);
        }
    }
}
