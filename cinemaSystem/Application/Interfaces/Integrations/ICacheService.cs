using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Integrations
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task<List<T>> GetAsync<T>(List<string> keys);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task UpdateAsync<T>(string key, T value);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}
