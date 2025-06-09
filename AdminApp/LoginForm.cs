using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace AdminApp
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("是否退出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                // 退出应用程序
                this.Close();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            errorProviderUser.Clear();
            errorProviderPwd.Clear();
            // 检查用户名和密码是否合法
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                errorProviderUser.SetError(txtUsername, "用户名不能为空");
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                errorProviderPwd.SetError(txtPassword, "密码不能为空");
                return;
            }

            // 从 JSON 文件中读取管理员的用户名和密码
            var credentials = LoadAdminCredentials();
            if (credentials == null)
            {
                MessageBox.Show("无法加载管理员凭据文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 验证用户名和密码
            if (txtUsername.Text != credentials.Username || txtPassword.Text != credentials.Password)
            {
                errorProviderUser.SetError(txtUsername, "用户名或密码错误");
                errorProviderPwd.SetError(txtPassword, "用户名或密码错误");
            }
            else
            {
                // 登录成功，打开主界面
                MainForm mainForm = new MainForm();
                mainForm.ShowDialog();
                this.Close();
            }
        }

        // 定义管理员凭据类
        public class AdminCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        // 加载管理员凭据的方法
        private AdminCredentials LoadAdminCredentials()
        {
            try
            {
                // 假设 JSON 文件路径为项目根目录下的 "admin_credentials.json"
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "admin_credentials.json");

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"找不到凭据文件：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // 读取 JSON 文件内容
                string json = File.ReadAllText(filePath);

                // 反序列化为 AdminCredentials 对象
                return JsonConvert.DeserializeObject<AdminCredentials>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载管理员凭据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
