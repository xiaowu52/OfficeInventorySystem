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

        // ���������
        public const string ITEM_CACHE_KEY = "items_list";
        public const string REQUEST_CACHE_KEY_PREFIX = "user_requests_";
        private const string PAGED_ITEMS_PREFIX = "paged_items_";
        private const string PAGED_REQUESTS_PREFIX = "paged_requests_";

        public RedisService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        // �ӻ����ȡ����
        public async Task<T> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        // �����ݴ��뻺��
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry;

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }

        // �ӻ������Ƴ�ָ����
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        // ��ȡ��Ʒ�����
        public string GetItemsCacheKey() => ITEM_CACHE_KEY;

        // ��ȡ�û����󻺴��
        public string GetUserRequestsCacheKey(string userId) => $"{REQUEST_CACHE_KEY_PREFIX}{userId}";

        // ��ȡ��ҳ��Ʒ�����
        public string GetPagedItemsCacheKey(int page, int pageSize, string searchTerm = "")
        {
            string key = $"{ITEM_CACHE_KEY}_page{page}_size{pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                key += $"_search{searchTerm}";
            return key;
        }

        // ��ȡ��ҳ�û����󻺴��
        public string GetPagedUserRequestsCacheKey(string userId, int page, int pageSize)
            => $"{REQUEST_CACHE_KEY_PREFIX}{userId}_page{page}_size{pageSize}";

        #region ����ʧЧ����

        // ʹ��Ʒ����ʧЧ
        public async Task InvalidateItemsCache()
        {
            await _cache.RemoveAsync(ITEM_CACHE_KEY);
            await InvalidatePagedItemsCache();
        }

        // ʹ�û����󻺴�ʧЧ
        public async Task InvalidateUserRequestsCache(string userId)
        {
            await _cache.RemoveAsync($"{REQUEST_CACHE_KEY_PREFIX}{userId}");
            await InvalidatePagedUserRequestsCache(userId);
        }

        // ʹ���з�ҳ��Ʒ����ʧЧ
        public async Task InvalidatePagedItemsCache()
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // ��ȡ����ƥ��Ļ����
                var keys = server.Keys(pattern: $"{ITEM_CACHE_KEY}_page*").ToArray();

                // ����ɾ����Щ��
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
            catch (Exception ex)
            {
                // ��¼�쳣�����׳�������Ӱ����Ҫҵ������
                Console.WriteLine($"Error invalidating paged items cache: {ex.Message}");
            }
        }

        // ʹָ���û������з�ҳ���󻺴�ʧЧ
        public async Task InvalidatePagedUserRequestsCache(string userId)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // ��ȡ���û����з�ҳ����Ļ����
                var keys = server.Keys(pattern: $"{REQUEST_CACHE_KEY_PREFIX}{userId}_page*").ToArray();

                // ����ɾ����Щ��
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

        // ������������ʹ��ҳ��Ʒ����ʧЧ
        public async Task InvalidatePagedItemsCacheBySearch(string searchTerm)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                // ��ȡ����ƥ�����������Ļ����
                var keys = server.Keys(pattern: $"{ITEM_CACHE_KEY}_page*_search{searchTerm}*").ToArray();

                // ����ɾ����Щ��
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

        #region ��������ʧЧ��Ϣ

        // ��������ʧЧ��Ϣ��Redisͨ��
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

        // ������Ʒ����ʧЧ��Ϣ
        public async Task PublishItemsCacheInvalidationAsync()
        {
            await PublishCacheInvalidationMessageAsync("items_updated");
        }

        // �����û����󻺴�ʧЧ��Ϣ
        public async Task PublishUserRequestsCacheInvalidationAsync(string userId)
        {
            await PublishCacheInvalidationMessageAsync($"user_requests_{userId}");
        }

        #endregion
    }
}
