using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace UserApp.Services
{
    public class RedisService
    {
        private readonly IDistributedCache _cache;
        private const string ITEM_CACHE_KEY = "items_list";
        private const string REQUEST_CACHE_KEY_PREFIX = "user_requests_";

        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry;

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task InvalidateItemsCache()
        {
            await _cache.RemoveAsync(ITEM_CACHE_KEY);
        }

        public async Task InvalidateUserRequestsCache(string userId)
        {
            await _cache.RemoveAsync($"{REQUEST_CACHE_KEY_PREFIX}{userId}");
        }

        public string GetItemsCacheKey() => ITEM_CACHE_KEY;

        public string GetUserRequestsCacheKey(string userId) => $"{REQUEST_CACHE_KEY_PREFIX}{userId}";
    }
} 