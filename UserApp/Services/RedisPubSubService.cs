using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UserApp.Services
{
    public class RedisPubSubService : BackgroundService
    {
        private readonly ILogger<RedisPubSubService> _logger;
        private readonly RedisService _redisService;
        private readonly IConnectionMultiplexer _redis;
        private const string CACHE_CHANNEL = "cache_invalidation";

        public RedisPubSubService(
            ILogger<RedisPubSubService> logger,
            RedisService redisService,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _redisService = redisService;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            
            try
            {
                var subscriber = _redis.GetSubscriber();
                
                // 订阅缓存失效通道
                await subscriber.SubscribeAsync(CACHE_CHANNEL, async (channel, message) =>
                {
                    _logger.LogInformation($"Received cache invalidation message: {message}");
                    
                    // 根据消息类型执行相应的缓存失效操作
                    if (message == "items_updated")
                    {
                        await _redisService.InvalidateItemsCache();
                        _logger.LogInformation("Items cache invalidated");
                    }
                    else if (message.ToString().StartsWith("user_requests_"))
                    {
                        var parts = message.ToString().Split('_');
                        if (parts.Length >= 3)
                        {
                            string userId = parts[2];
                            await _redisService.InvalidateUserRequestsCache(userId);
                            _logger.LogInformation($"User requests cache invalidated for user {userId}");
                        }
                    }
                });
                
                _logger.LogInformation("Subscribed to Redis cache invalidation channel");
                
                // 保持服务运行
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Redis pub/sub service");
            }
        }
    }
} 