using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;
using UserApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using UserApp.Services;
using System.Text.Json;

namespace UserApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redisService;

        public HomeController(IConfiguration configuration, IDistributedCache cache, RedisService redisService)
        {
            _cache = cache;
            _configuration = configuration;
            _redisService = redisService;
        }

        public IActionResult Index()
        {
            string? userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = userName;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> StockQuery()
        {
            // Try to get items from Redis cache first
            var items = await _redisService.GetAsync<List<Item>>(_redisService.GetItemsCacheKey());
            
            if (items == null)
            {
                // If not in cache, get from database
                string? connectionString = _configuration.GetConnectionString("MySqlConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'MySqlConnection' is not configured.");
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT item_id, name, category, origin, specification, model, stock_quantity FROM item";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        items = new List<Item>();
                        while (reader.Read())
                        {
                            items.Add(new Item
                            {
                                item_id = reader.GetString("item_id"),
                                name = reader.GetString("name"),
                                category = reader.GetString("category"),
                                origin = reader.IsDBNull(reader.GetOrdinal("origin")) ? string.Empty : reader.GetString("origin"),
                                specification = reader.GetString("specification"),
                                model = reader.GetString("model"),
                                stock_quantity = reader.GetInt32("stock_quantity")
                            });
                        }
                    }
                }
                
                // Cache the items for 10 minutes
                if (items.Count > 0)
                {
                    await _redisService.SetAsync(_redisService.GetItemsCacheKey(), items, TimeSpan.FromMinutes(10));
                }
            }
            
            return View(items);
        }
        
        private async Task<List<RequestViewModel>> GetUserRequests(string userId)
        {
            // Try to get from cache first
            var cacheKey = _redisService.GetUserRequestsCacheKey(userId);
            var requests = await _redisService.GetAsync<List<RequestViewModel>>(cacheKey);
            
            if (requests == null)
            {
                requests = new List<RequestViewModel>();
                string connectionString = _configuration.GetConnectionString("MySqlConnection");

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"SELECT r.*, i.name as item_name 
                         FROM request r
                         JOIN item i ON r.item_id = i.item_id
                         WHERE r.user_id = @UserId
                         ORDER BY r.request_date DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                requests.Add(new RequestViewModel
                                {
                                    ItemName = reader.GetString("item_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    RequestDate = reader.GetDateTime("request_date"),
                                    Status = (RequestStatus)Enum.Parse(typeof(RequestStatus), reader.GetString("status"))
                                });
                            }
                        }
                    }
                }
                
                // Cache user requests for 5 minutes
                if (requests.Count > 0)
                {
                    await _redisService.SetAsync(cacheKey, requests, TimeSpan.FromMinutes(5));
                }
            }
            
            return requests;
        }
        
        [HttpGet]
        public async Task<IActionResult> RequestItem()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get available items - try from cache first
            var items = await _redisService.GetAsync<List<Item>>(_redisService.GetItemsCacheKey());
            
            if (items == null)
            {
                items = new List<Item>();
                string connectionString = _configuration.GetConnectionString("MySqlConnection");
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT item_id, name FROM item";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Item
                            {
                                item_id = reader.GetString("item_id"),
                                name = reader.GetString("name")
                            });
                        }
                    }
                }
                
                // Cache items
                if (items.Count > 0)
                {
                    await _redisService.SetAsync(_redisService.GetItemsCacheKey(), items, TimeSpan.FromMinutes(10));
                }
            }

            // Get user request history
            var requests = await GetUserRequests(userId);

            var model = new RequestViewModel
            {
                AvailableItems = items,
                RequestHistory = requests
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> RequestItem(string itemId, int quantity)
        {
            string userId = HttpContext.Session.GetString("UserID");
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            string connectionString = _configuration.GetConnectionString("MySqlConnection");
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO request (item_id, quantity, user_id, user_name, request_date, status) VALUES (@ItemId, @Quantity, @UserID, @UserName, @Date, '')";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemId", itemId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
            
            // Invalidate the user's request cache
            await _redisService.InvalidateUserRequestsCache(userId);
            
            return RedirectToAction("Index");
        }
    }
}
