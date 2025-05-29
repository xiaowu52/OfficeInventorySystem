using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

namespace AdminApp
{
    public partial class AddUserForm : Form
    {
        public AddUserForm()
        {
            InitializeComponent();
        }
        public class User
        {
            public string UserID { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string Gender { get; set; }
            public DateTime BirthDate { get; set; }
            public string PhoneNumber { get; set; }
        }

        private List<User> users;
        string jsonFilePath = ConfigurationManager.AppSettings["UserFilePath"]; // 从 App.config 中读取路径
        private void LoadExistingUsers()
        {
            // 加载现有用户数据
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                users = JsonConvert.DeserializeObject<List<User>>(jsonData) ?? new List<User>();
            }
            else
            {
                users = new List<User>();
            }
        }
        private void AddUserForm_Load(object sender, EventArgs e)
        {
            LoadExistingUsers();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ePID.Clear(); // 清除错误提示
            //添加用户ID存在检测  
            string userId = txtUserID.Text.Trim();
            if (string.IsNullOrEmpty(userId))
            {
                //用errorProvide提示用户ID不能为空
                ePID.SetError(txtUserID, "用户ID不能为空！");
            }
            // 检查用户ID是否已存在  
            if (UserExists(userId))
            {
                ePID.SetError(txtUserID, "用户ID已存在!");
            }
        }

        // 添加 UserExists 方法  
        private bool UserExists(string userId)
        {
            if (users != null)
            {
                // 检查用户ID是否已存在
                return users.Any(u => u.UserID == userId);
            }
            return false;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            ePPassword.Clear(); // 清除错误提示
            //判断是否为空
            string password = txtPassword.Text.Trim();
            if (string.IsNullOrEmpty(password))
            {
                //用errorProvide提示密码不能为空
                ePPassword.SetError(txtPassword, "密码不能为空！");
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            ePName.Clear();
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                //用errorProvide提示姓名不能为空
                ePName.SetError(txtName, "姓名不能为空！");
            }
        }

        private void txtPhoneNumber_TextChanged(object sender, EventArgs e)
        {
            ePTelephone.Clear();
            string phoneNumber = txtPhoneNumber.Text.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                //用errorProvide提示联系电话不能为空
                ePTelephone.SetError(txtPhoneNumber, "联系电话不能为空！");
            }
            // 检查电话号码格式
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{11}$"))
            {
                ePTelephone.SetError(txtPhoneNumber, "联系电话格式不正确！");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 验证用户输入
            if (string.IsNullOrWhiteSpace(txtUserID.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("用户ID和密码不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //姓名不能为空
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("姓名不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //联系电话不能为空
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                MessageBox.Show("联系电话不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查电话号码格式
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtPhoneNumber.Text.Trim(), @"^\d{11}$"))
            {
                MessageBox.Show("联系电话格式不正确！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //选择性别
            if (cmbGender.SelectedItem == null)
            {
                MessageBox.Show("请选择性别！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            

            // 创建新用户对象
            User newUser = new User
            {
                UserID = txtUserID.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                UserName = txtName.Text.Trim(),
                Gender = cmbGender.SelectedItem?.ToString(),
                BirthDate = dtpBirthDate.Value.Date,
                PhoneNumber = txtPhoneNumber.Text.Trim()
            };

            // 检查用户ID是否已存在
            if (users.Exists(u => u.UserID == newUser.UserID))
            {
                MessageBox.Show("用户ID已存在，请使用其他ID！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 添加新用户到列表
            users.Add(newUser);

            // 保存到 JSON 文件
            try
            {
                string jsonData = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(jsonFilePath, jsonData);
                MessageBox.Show("用户添加成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // 关闭窗口
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //判断是否为空，否则提示有未保存数据
            if (string.IsNullOrEmpty(txtUserID.Text) && string.IsNullOrEmpty(txtPassword.Text) && string.IsNullOrEmpty(txtName.Text) && string.IsNullOrEmpty(txtPhoneNumber.Text))
            {
                this.Close(); // 关闭窗口
            }
            else
            {
                DialogResult result = MessageBox.Show("是否放弃当前操作？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    this.Close(); // 关闭窗口
                }
                else
                {
                    return;
                }
            }
        }
    }
}
