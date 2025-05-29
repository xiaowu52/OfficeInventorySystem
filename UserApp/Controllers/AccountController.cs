using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace UserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        List<UserApp.Models.User> LoadUsers()
        {
            string? filePath = _configuration["UserFilePath"];
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<UserApp.Models.User>>(json) ?? new List<UserApp.Models.User>();
            }
            return new List<UserApp.Models.User>();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "用户名和密码不能为空！";
                return View();
            }

            // 加载用户数据
            List<UserApp.Models.User> users = LoadUsers();

            // 验证用户名和密码
            var user = users.FirstOrDefault(u => u.UserID == userId && u.Password == password);
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
