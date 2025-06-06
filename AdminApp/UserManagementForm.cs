using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using MySql.Data.MySqlClient;

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
            public string UserID { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string Gender { get; set; }
            public DateTime BirthDate { get; set; }
            public string PhoneNumber { get; set; }
        }

        private BindingList<User> users = new BindingList<User>();
        private bool isDataModified = false;
        private string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private void LoadUserData()
        {
            try
            {
                // 清空当前用户列表
                users.Clear();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT user_id, password, user_name, gender, birth_date, phone_number FROM user";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    UserID = reader.GetString("user_id"),
                                    Password = reader.GetString("password"),
                                    UserName = reader.GetString("user_name"),
                                    Gender = reader.GetString("gender"),
                                    BirthDate = reader.GetDateTime("birth_date"),
                                    PhoneNumber = reader.GetString("phone_number")
                                };

                                users.Add(user);
                            }
                        }
                    }
                }

                // 更新DataGridView
                dgvUsers.DataSource = null;
                dgvUsers.DataSource = users;

                // 隐藏密码列  
                if (dgvUsers.Columns["Password"] != null)
                {
                    dgvUsers.Columns["Password"].Visible = false;
                }

                isDataModified = false; // 重置修改标记
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();

                            // 删除用户
                            string deleteQuery = "DELETE FROM user WHERE user_id = @userId";
                            using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@userId", selectedUser.UserID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    // 从列表中移除已删除的用户
                                    users.Remove(selectedUser);
                                    MessageBox.Show("用户已成功删除！", "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("删除用户失败，用户可能已被删除。", "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除用户时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的用户！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool SaveUserData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 使用事务确保所有修改要么全部成功，要么全部失败
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (User user in users)
                            {
                                // 检查用户是否存在
                                string checkQuery = "SELECT COUNT(*) FROM user WHERE user_id = @userId";
                                using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection, transaction))
                                {
                                    checkCommand.Parameters.AddWithValue("@userId", user.UserID);
                                    int userExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                                    if (userExists > 0)
                                    {
                                        // 更新现有用户
                                        string updateQuery = @"
                                            UPDATE user 
                                            SET password = @password, user_name = @userName, gender = @gender, 
                                                birth_date = @birthDate, phone_number = @phoneNumber 
                                            WHERE user_id = @userId";

                                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                                        {
                                            updateCommand.Parameters.AddWithValue("@userId", user.UserID);
                                            updateCommand.Parameters.AddWithValue("@password", user.Password);
                                            updateCommand.Parameters.AddWithValue("@userName", user.UserName);
                                            updateCommand.Parameters.AddWithValue("@gender", user.Gender);
                                            updateCommand.Parameters.AddWithValue("@birthDate", user.BirthDate);
                                            updateCommand.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);

                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        // 插入新用户
                                        string insertQuery = @"
                                            INSERT INTO user (user_id, password, user_name, gender, birth_date, phone_number) 
                                            VALUES (@userId, @password, @userName, @gender, @birthDate, @phoneNumber)";

                                        using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection, transaction))
                                        {
                                            insertCommand.Parameters.AddWithValue("@userId", user.UserID);
                                            insertCommand.Parameters.AddWithValue("@password", user.Password);
                                            insertCommand.Parameters.AddWithValue("@userName", user.UserName);
                                            insertCommand.Parameters.AddWithValue("@gender", user.Gender);
                                            insertCommand.Parameters.AddWithValue("@birthDate", user.BirthDate);
                                            insertCommand.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber);

                                            insertCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // 所有操作都成功，提交事务
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            // 发生错误，回滚事务
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存用户数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnModifyconfirm_Click(object sender, EventArgs e)
        {
            //直接在dgv修改的数据保存到数据库
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
                        if (updatedUsers.Count(u => u.UserID == user.UserID) > 1)
                        {
                            MessageBox.Show("用户ID不能重复！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //性别不能为空
                        if (string.IsNullOrEmpty(user.Gender))
                        {
                            MessageBox.Show("性别不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        //性别为男女，不能为其它任意值
                        if (user.Gender != "男" && user.Gender != "女")
                        {
                            MessageBox.Show("性别只能为男或女！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    users = updatedUsers; // 更新用户列表

                    // 保存更新后的用户数据到数据库
                    if (SaveUserData())
                    {
                        MessageBox.Show("用户信息已成功保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        isDataModified = false; // 重置修改标志
                    }
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

            // 检查ID是否重复
            if (columnName == "UserID")
            {
                string userId = e.FormattedValue.ToString().Trim();

                // 检查是否在当前列表中重复
                int duplicateCount = 0;
                foreach (DataGridViewRow row in dgvUsers.Rows)
                {
                    if (row.Index != e.RowIndex && row.Cells[e.ColumnIndex].Value?.ToString() == userId)
                    {
                        duplicateCount++;
                    }
                }

                if (duplicateCount > 0)
                {
                    MessageBox.Show("用户ID不能重复！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgvUsers.CancelEdit();
                }
                else
                {
                    // 检查是否与数据库中的其他记录重复(当更改ID时需要检查)
                    try
                    {
                        User currentUser = dgvUsers.Rows[e.RowIndex].DataBoundItem as User;
                        if (currentUser != null && currentUser.UserID != userId)
                        {
                            // 只在ID改变时才检查数据库
                            using (MySqlConnection connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();
                                string query = "SELECT COUNT(*) FROM user WHERE user_id = @userId";
                                using (MySqlCommand command = new MySqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@userId", userId);
                                    int count = Convert.ToInt32(command.ExecuteScalar());

                                    if (count > 0)
                                    {
                                        MessageBox.Show("用户ID已存在于数据库中！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        dgvUsers.CancelEdit();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"验证用户ID时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
