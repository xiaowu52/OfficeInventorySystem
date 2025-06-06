using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Linq;

namespace UserApp.Services
{
    public class RedisService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        // 缓存键常量
        public const string ITEM_CACHE_KEY = "items_list";
        public const string REQUEST_CACHE_KEY_PREFIX = "user_requests_";
        private const string PAGED_ITEMS_PREFIX = "paged_items_";
        private const string PAGED_REQUESTS_PREFIX = "paged_requests_";

        public RedisService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        // 从缓存获取数据
        public async Task<T> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        // 将数据存入缓存
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry;

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }

        // 从缓存中移除指定键
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        // 获取物品缓存键
        public string GetItemsCacheKey() => ITEM_CACHE_KEY;

        // 获取用户请求缓存键
        public string GetUserRequestsCacheKey(string userId) => $"{REQUEST_CACHE_KEY_PREFIX}{userId}";

        // 获取分页物品缓存键
        public string GetPagedItemsCacheKey(int page, int pageSize, string searchTerm = "")
        {
            string key = $"{ITEM_CACHE_KEY}_page{page}_size{pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                key += $"_search{searchTerm}";
            return key;
        }

        // 获取分页用户请求缓存键
        public string GetPagedUserRequestsCacheKey(string userId, int page, int pageSize)
            => $"{REQUEST_CACHE_KEY_PREFIX}{userId}_page{page}_size{pageSize}";

        #region 缓存失效方法

        // 使物品缓存失效
        public async Task InvalidateItemsCache()
        {
            await _cache.RemoveAsync(ITEM_CACHE_KEY);
            await InvalidatePagedItemsCache();
        }

        // 使用户请求缓存失效
        public async Task InvalidateUserRequestsCache(string userId)
        {
            await _cache.RemoveAsync($"{REQUEST_CACHE_KEY_PREFIX}{userId}");
            await InvalidatePagedUserRequestsCache(userId);
        }

        // 使所有分页物品缓存失效
        public async Task InvalidatePagedItemsCache()
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // 获取所有匹配的缓存键
                var keys = server.Keys(pattern: $"{ITEM_CACHE_KEY}_page*").ToArray();

                // 批量删除这些键
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，避免影响主要业务流程
                Console.WriteLine($"Error invalidating paged items cache: {ex.Message}");
            }
        }

        // 使指定用户的所有分页请求缓存失效
        public async Task InvalidatePagedUserRequestsCache(string userId)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // 获取该用户所有分页请求的缓存键
                var keys = server.Keys(pattern: $"{REQUEST_CACHE_KEY_PREFIX}{userId}_page*").ToArray();

                // 批量删除这些键
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invalidating paged user requests cache: {ex.Message}");
            }
        }

        // 根据搜索条件使分页物品缓存失效
        public async Task InvalidatePagedItemsCacheBySearch(string searchTerm)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // 获取所有匹配搜索条件的缓存键
                var keys = server.Keys(pattern: $"{ITEM_CACHE_KEY}_page*_search{searchTerm}*").ToArray();

                // 批量删除这些键
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invalidating paged items cache by search: {ex.Message}");
            }
        }

        #endregion

        #region 发布缓存失效消息

        // 发布缓存失效消息到Redis通道
        public async Task PublishCacheInvalidationMessageAsync(string message)
        {
            try
            {
                var subscriber = _connectionMultiplexer.GetSubscriber();
                await subscriber.PublishAsync("cache_invalidation", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing cache invalidation message: {ex.Message}");
            }
        }

        // 发布物品缓存失效消息
        public async Task PublishItemsCacheInvalidationAsync()
        {
            await PublishCacheInvalidationMessageAsync("items_updated");
        }

        // 发布用户请求缓存失效消息
        public async Task PublishUserRequestsCacheInvalidationAsync(string userId)
        {
            await PublishCacheInvalidationMessageAsync($"user_requests_{userId}");
        }

        #endregion
    }
}
