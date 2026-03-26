namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Redis distributed cache abstraction.
    /// </summary>
    public interface ICacheService
    {
        Task<bool> ExistsAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task<List<T>> GetAsync<T>(List<string> keys);
        Task RemoveAsync(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
        Task UpdateAsync<T>(string key, T value);
    }
}
