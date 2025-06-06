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
        private const int DefaultPageSize = 10; // Ĭ��ÿҳ��ʾ10����¼

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
        public async Task<IActionResult> StockQuery(int page = 1, int pageSize = DefaultPageSize, string searchTerm = "")
        {
            // ������ҳ��ͼģ��
            var paginatedModel = new PaginatedList<Item>
            {
                PageIndex = page,
                PageSize = pageSize
            };

            // ���Դӻ����ȡ����
            string cacheKey = $"{_redisService.GetItemsCacheKey()}_page{page}_size{pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                cacheKey += $"_search{searchTerm}";

            var cachedResult = await _redisService.GetAsync<PaginatedList<Item>>(cacheKey);

            if (cachedResult != null)
            {
                return View(cachedResult);
            }

            // �������û�У������ݿ��ѯ
            string? connectionString = _configuration.GetConnectionString("MySqlConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'MySqlConnection' is not configured.");
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // ��ȡ�ܼ�¼��
                string countQuery = "SELECT COUNT(*) FROM item";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countQuery += " WHERE name LIKE @SearchTerm OR item_id LIKE @SearchTerm";
                }

                using (MySqlCommand countCommand = new MySqlCommand(countQuery, connection))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        countCommand.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    }
                    paginatedModel.TotalItems = Convert.ToInt32(countCommand.ExecuteScalar());
                }

                // ������ҳ��
                paginatedModel.TotalPages = (int)Math.Ceiling(paginatedModel.TotalItems / (double)pageSize);

                // ��ѯ����
                string dataQuery = "SELECT item_id, name, category, origin, specification, model, stock_quantity FROM item";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    dataQuery += " WHERE name LIKE @SearchTerm OR item_id LIKE @SearchTerm";
                }
                dataQuery += " ORDER BY item_id LIMIT @Skip, @Take";

                using (MySqlCommand command = new MySqlCommand(dataQuery, connection))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    }
                    command.Parameters.AddWithValue("@Skip", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@Take", pageSize);

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
                        paginatedModel.Items = items;
                    }
                }
            }

            // ������
            if (paginatedModel.Items.Count > 0)
            {
                await _redisService.SetAsync(cacheKey, paginatedModel, TimeSpan.FromMinutes(5));
            }

            // ����������������ͼ
            ViewBag.SearchTerm = searchTerm;

            return View(paginatedModel);
        }

        private async Task<PaginatedList<RequestViewModel>> GetUserRequests(string userId, int page = 1, int pageSize = DefaultPageSize)
        {
            var result = new PaginatedList<RequestViewModel>
            {
                PageIndex = page,
                PageSize = pageSize
            };

            // ���Դӻ����ȡ
            var cacheKey = $"{_redisService.GetUserRequestsCacheKey(userId)}_page{page}_size{pageSize}";
            var cachedResult = await _redisService.GetAsync<PaginatedList<RequestViewModel>>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            // �����ݿ��ȡ
            string connectionString = _configuration.GetConnectionString("MySqlConnection");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // ��ȡ�ܼ�¼��
                string countQuery = "SELECT COUNT(*) FROM request WHERE user_id = @UserId";
                using (var countCommand = new MySqlCommand(countQuery, connection))
                {
                    countCommand.Parameters.AddWithValue("@UserId", userId);
                    result.TotalItems = Convert.ToInt32(countCommand.ExecuteScalar());
                }

                // ������ҳ��
                result.TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

                // ��ѯ��ǰҳ����
                var query = @"SELECT r.*, i.name as item_name 
                     FROM request r
                     JOIN item i ON r.item_id = i.item_id
                     WHERE r.user_id = @UserId
                     ORDER BY r.request_date DESC
                     LIMIT @Skip, @Take";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Skip", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@Take", pageSize);

                    using (var reader = command.ExecuteReader())
                    {
                        var requests = new List<RequestViewModel>();
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
                        result.Items = requests;
                    }
                }
            }

            // ������
            if (result.Items.Count > 0)
            {
                await _redisService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> RequestItem(int page = 1, int pageSize = DefaultPageSize)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ��ȡ����ҳ��������ʷ��¼
            var requests = await GetUserRequests(userId, page, pageSize);

            // ֱ�ӷ���������ʷ��¼�����ٰ�����Ʒ�б���������
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest(RequestViewModel model)
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
                    command.Parameters.AddWithValue("@ItemId", model.SelectedItemId);
                    command.Parameters.AddWithValue("@Quantity", model.Quantity);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }

            // ʹ������ػ���ʧЧ
            await _redisService.InvalidateUserRequestsCache(userId);

            // ��ӳɹ���Ϣ
            TempData["SuccessMessage"] = "���������ѳɹ��ύ��";

            // ���ص�����ѯҳ��
            return RedirectToAction("StockQuery");
        }

        [HttpPost]
        public async Task<IActionResult> RequestItem(RequestViewModel model)
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
                    command.Parameters.AddWithValue("@ItemId", model.SelectedItemId);
                    command.Parameters.AddWithValue("@Quantity", model.Quantity);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }

            // ʹ������ػ���ʧЧ
            await _redisService.InvalidateUserRequestsCache(userId);

            return RedirectToAction("RequestItem");
        }
    }
}
