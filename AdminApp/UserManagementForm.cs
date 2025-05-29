using Newtonsoft.Json;
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
using System.Xml.Linq;
using System.Diagnostics;
using System.Configuration;

namespace AdminApp
{
    
    public partial class UserManagementForm : Form
    {
        
        public UserManagementForm()
        {
            InitializeComponent();
        }
        public class User
        {
            [JsonProperty("UserID")]  // JSON键映射
            public string UserID { get; set; }

            [JsonProperty("Password")]
            public string Password { get; set; }

            [JsonProperty("UserName")]
            public string UserName { get; set; }

            [JsonProperty("Gender")]
            public string Gender { get; set; }

            [JsonProperty("BirthDate")]
            public DateTime BirthDate { get; set; }

            [JsonProperty("PhoneNumber")]
            public string PhoneNumber { get; set; }
        }

        private List<User> users;
        private bool isDataModified = false;
        string jsonFilePath = ConfigurationManager.AppSettings["UserFilePath"]; // 从 App.config 中读取路径

        private void LoadUserData()
        {
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    // 读取 JSON 文件内容  
                    string jsonData = File.ReadAllText(jsonFilePath);
                    // 反序列化为用户列表  
                    users = JsonConvert.DeserializeObject<List<User>>(jsonData);
                    dgvUsers.DataSource = new BindingList<User>(users);
                    // 隐藏密码列  
                    dgvUsers.Columns["Password"].Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("用户数据文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UserManagementForm_Load(object sender, EventArgs e)
        {
            dgvUsers.AutoGenerateColumns = false;
            LoadUserData();
        }

        private void btnBackToMain_Click(object sender, EventArgs e)
        {
            //如果dgv有修改数据且未保存提示
            if (isDataModified)
            {
                var result = MessageBox.Show("检测到未保存的修改，是否保存？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // 保存数据
                    btnModifyconfirm_Click(sender, e);
                    //保存任务失败
                    if (isDataModified)
                    {
                        MessageBox.Show("保存数据失败，请检查数据是否违规", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    // 取消关闭
                    return;
                }
            }
            this.Close();
        }
        
        private void btnAdduser_Click(object sender, EventArgs e)
        {
            //进入添加用户界面
            AddUserForm addUserForm = new AddUserForm();
            addUserForm.ShowDialog(this); // 显示为模态对话框
            //加载更新后的用户数据
            LoadUserData();
        }

        private void btnDeleteuser_Click(object sender, EventArgs e)
        {
            // 检查是否选中了一行
            if (dgvUsers.CurrentRow != null && dgvUsers.CurrentRow.DataBoundItem is User selectedUser)
            {
                // 显示确认删除提示
                var confirmResult = MessageBox.Show($"确定要删除用户 {selectedUser.UserName} 吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmResult == DialogResult.Yes)
                {
                    // 从用户列表中移除选中用户
                    users.Remove(selectedUser);
              
                    // 重新绑定数据到 DataGridView
                    dgvUsers.DataSource = new BindingList<User>(users);

                    // 保存更新后的用户数据到文件
                    SaveUserData();

                    MessageBox.Show("用户已成功删除！", "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的用户！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveUserData()
        {
            try
            {
                // 序列化用户列表为 JSON  
                string jsonData = JsonConvert.SerializeObject(users, Formatting.Indented);

                // 写入 JSON 文件  
                File.WriteAllText(jsonFilePath, jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModifyconfirm_Click(object sender, EventArgs e)
        {
            //直接在dgv修改的数据保存到文件中
            try
            {
                // 将 DataGridView 的数据同步到用户列表
                BindingList<User> updatedUsers = dgvUsers.DataSource as BindingList<User>;
                if (updatedUsers != null)
                {
                    //验证输入
                    foreach (var user in updatedUsers)
                    {
                        if (string.IsNullOrWhiteSpace(user.UserID) || string.IsNullOrWhiteSpace(user.Password))
                        {
                            MessageBox.Show("用户ID和密码不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(user.UserName))
                        {
                            MessageBox.Show("姓名不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                        {
                            MessageBox.Show("联系电话不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        // 检查电话号码格式
                        if (!System.Text.RegularExpressions.Regex.IsMatch(user.PhoneNumber.Trim(), @"^\d{11}$"))
                        {
                            MessageBox.Show("联系电话格式不正确！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //ID不能重复
                        if (users.Any(u => u.UserID == user.UserID && u != user))
                        {
                            MessageBox.Show("用户ID不能重复！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //性别不能为空
                        if(string.IsNullOrEmpty(user.Gender))
                        {
                            MessageBox.Show("性别不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //性别为男女，不能为其它任意值
                        if(user.Gender != "男" &&  user.Gender != "女")
                        {
                            MessageBox.Show("性别只能为男或女！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    users = updatedUsers.ToList();

                    // 保存更新后的用户数据到 JSON 文件
                    SaveUserData();

                    MessageBox.Show("用户信息已成功保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    isDataModified = false; // 重置修改标志
                }
                else
                {
                    MessageBox.Show("无法获取用户数据，请检查数据绑定！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //对选择的单元格以及行选将其姓名输出到lblUsername
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var selectedUser = dgvUsers.Rows[e.RowIndex].DataBoundItem as User;
                if (selectedUser != null)
                {
                    lblUsername.Text = $"{selectedUser.UserName}";
                }
            }
        }

        private void dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // 如果 DataGridView 当前未处于编辑状态，直接返回
            if (!dgvUsers.IsCurrentCellDirty)
            {
                return;
            }
            // 获取当前列名
            string columnName = dgvUsers.Columns[e.ColumnIndex].Name;

            // 验证电话号码格式
            if (columnName == "PhoneNumber")
            {
                string phoneNumber = e.FormattedValue.ToString().Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{11}$"))
                {
                    MessageBox.Show("联系电话格式不正确！请输入11位数字。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvUsers.CancelEdit();
                }
            }

            // 验证出生日期格式
            if (columnName == "BirthDate")
            {
                if (!DateTime.TryParse(e.FormattedValue.ToString(), out _))
                {
                    MessageBox.Show("出生日期格式不正确！请输入有效的日期。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvUsers.CancelEdit();
                }
            }

            // 验证性别
            if (columnName == "Gender")
            {
                string gender = e.FormattedValue.ToString().Trim();
                if (gender != "男" && gender != "女")
                {
                    MessageBox.Show("性别只能为“男”或“女”。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvUsers.CancelEdit();
                }
            }

            //ID是否重复
            if (columnName == "UserID")
            {
                string userId = e.FormattedValue.ToString().Trim();
                if (users.Any(u => u.UserID == userId && dgvUsers.Rows[e.RowIndex].DataBoundItem != u))
                {
                    MessageBox.Show("用户ID不能重复！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                   dgvUsers.CancelEdit();
                }
            }
        }

        private void dgv_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvUsers.IsCurrentCellDirty) // 检查当前单元格是否真的被修改
            {
                isDataModified = true;
            }
        }
        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvUsers.IsCurrentCellDirty) // 检查当前单元格是否真的被修改
            {
                isDataModified = true;
            }
        }

        private void dgv_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var selectedUser = dgvUsers.Rows[e.RowIndex].DataBoundItem as User;
            if (selectedUser != null)
            {
                lblUsername.Text = $"{selectedUser.UserName}";
            }
        }
    }
}
