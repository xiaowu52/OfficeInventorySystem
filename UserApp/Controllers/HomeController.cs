using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;
using UserApp.Models;


namespace UserApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public IActionResult StockQuery()
        {
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
                    var items = new List<Item>();
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
                    return View(items);
                }
            }
        }
        private List<RequestViewModel> GetUserRequests(string userId)
        {
            var requests = new List<RequestViewModel>();
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
            return requests;
        }
        [HttpGet]
        public IActionResult RequestItem()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // 获取可选物品列表
            var items = new List<Item>();
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

            // 获取用户申请记录
            var requests = GetUserRequests(userId);

            var model = new RequestViewModel
            {
                AvailableItems = items,
                RequestHistory = requests
            };

            return View(model);
        }
        [HttpPost]
        public IActionResult RequestItem(string itemId, int quantity)
        {
            string userId = HttpContext.Session.GetString("UserID");
            //根据用户ID获取用户名
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            string connectionString = _configuration.GetConnectionString("MySqlConnection");
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO request (item_id, quantity, user_id, user_name, request_date, status) VALUES (@ItemId, @Quantity, @UserID, @UserName, @Date, '申请中')";
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
            return RedirectToAction("Index");
        }
    }
}
