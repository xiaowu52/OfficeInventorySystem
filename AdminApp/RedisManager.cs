using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace AdminApp
{
    /// <summary>
    /// Redis缓存管理类，用于处理所有Redis缓存相关操作
    /// </summary>
    public static class RedisManager
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
        private static readonly string RedisConnectionString;

        // 共享的缓存键，确保与UserApp使用相同的键
        public const string ITEM_CACHE_KEY = "UserApp_items_list";
        public const string REQUEST_CACHE_KEY_PREFIX = "UserApp_user_requests_";

        // 初始化Redis连接
        static RedisManager()
        {
            try
            {
                // 从配置文件获取Redis连接字符串，如果没有则使用默认值
                RedisConnectionString = System.Configuration.ConfigurationManager
                    .AppSettings["RedisConnection"] ?? "localhost:6379";

                LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                    ConnectionMultiplexer.Connect(RedisConnectionString));
            }
            catch (Exception ex)
            {
                // 记录错误但不中断应用程序启动
                System.Diagnostics.Debug.WriteLine($"Redis初始化错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取Redis连接
        /// </summary>
        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        /// <summary>
        /// 获取Redis数据库
        /// </summary>
        public static IDatabase Database => Connection.GetDatabase();

        /// <summary>
        /// 检查Redis是否可用
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                try
                {
                    return LazyConnection.IsValueCreated && Connection.IsConnected;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 从缓存获取数据
        /// </summary>
        /// <typeparam name="T">要返回的类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存的对象，如果不存在则返回默认值</returns>
        public static T Get<T>(string key)
        {
            if (!IsAvailable) return default;

            try
            {
                string value = Database.StringGet(key);
                if (string.IsNullOrEmpty(value))
                    return default;

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis获取数据错误: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">要缓存的对象类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">要缓存的对象</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否成功</returns>
        public static bool Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!IsAvailable) return false;

            try
            {
                string serialized = JsonConvert.SerializeObject(value);
                return Database.StringSet(key, serialized, expiry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis设置数据错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从缓存移除指定的键
        /// </summary>
        /// <param name="key">要移除的缓存键</param>
        /// <returns>是否成功</returns>
        public static bool Remove(string key)
        {
            if (!IsAvailable) return false;

            try
            {
                return Database.KeyDelete(key);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis删除键错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 针对DataTable的专用缓存方法
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="dataTable">要缓存的DataTable对象</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否成功</returns>
        public static bool SetDataTable(string key, DataTable dataTable, TimeSpan? expiry = null)
        {
            if (!IsAvailable) return false;

            try
            {
                // 将DataTable转为JSON
                string serialized = JsonConvert.SerializeObject(dataTable);
                return Database.StringSet(key, serialized, expiry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis缓存DataTable错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从缓存获取DataTable
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存的DataTable，如果不存在则返回null</returns>
        public static DataTable GetDataTable(string key)
        {
            if (!IsAvailable) return null;

            try
            {
                string value = Database.StringGet(key);
                if (string.IsNullOrEmpty(value))
                    return null;

                return JsonConvert.DeserializeObject<DataTable>(value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis获取DataTable错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static bool ClearAllCache()
        {
            if (!IsAvailable) return false;

            try
            {
                var endpoints = Connection.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    var server = Connection.GetServer(endpoint);
                    server.FlushAllDatabases();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis清除所有缓存错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 当物品数据变更时，清除物品缓存
        /// </summary>
        public static bool InvalidateItemsCache()
        {
            return Remove(ITEM_CACHE_KEY);
        }
        
        /// <summary>
        /// 当用户请求状态变更时，清除特定用户的请求缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        public static bool InvalidateUserRequestsCache(string userId)
        {
            return Remove($"{REQUEST_CACHE_KEY_PREFIX}{userId}");
        }
        
        /// <summary>
        /// 发布消息到Redis通道，通知其他应用程序缓存已更新
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="message">消息内容</param>
        public static bool PublishMessage(string channel, string message)
        {
            if (!IsAvailable) return false;
            
            try
            {
                var subscriber = Connection.GetSubscriber();
                subscriber.Publish(channel, message);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis发布消息错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 订阅Redis通道
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="handler">消息处理函数</param>
        public static void SubscribeToChannel(string channel, Action<string> handler)
        {
            if (!IsAvailable) return;
            
            try
            {
                var subscriber = Connection.GetSubscriber();
                subscriber.Subscribe(channel, (_, message) => handler(message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis订阅通道错误: {ex.Message}");
            }
        }
    }
}
