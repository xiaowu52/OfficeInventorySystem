using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using UserApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using UserApp.Services;
using System.Data;

namespace UserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly RedisService _redisService;

        public AccountController(IConfiguration configuration, RedisService redisService)
        {
            _configuration = configuration;
            _redisService = redisService;
        }

        private async Task<User> GetUserFromDatabaseAsync(string userId, string password)
        {
            string connectionString = _configuration.GetConnectionString("MySqlConnection");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT * FROM user WHERE user_id = @UserId AND password = @Password";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserID = reader.GetString("user_id"),
                                Password = reader.GetString("password"),
                                UserName = reader.GetString("user_name"),
                                Gender = reader.GetString("gender"),
                                BirthDate = reader.GetDateTime("birth_date"),
                                PhoneNumber = reader.GetString("phone_number")
                            };
                        }
                    }
                }
            }
            return null;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "用户名和密码不能为空！";
                return View();
            }

            // 从数据库验证用户
            var user = await GetUserFromDatabaseAsync(userId, password);
            if (user != null)
            {
                // 登录成功，保存用户信息到会话
                HttpContext.Session.SetString("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", user.UserName);
                return RedirectToAction("Index", "Home");
            }

            // 登录失败
            ViewBag.ErrorMessage = "用户名或密码错误！";
            return View();
        }
    }
}
