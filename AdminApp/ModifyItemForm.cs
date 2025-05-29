using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AdminApp
{
    public partial class ModifyItemForm : Form
    {
        private string itemId; // 用于存储物品 ID
        public ModifyItemForm(string itemId, string name, string category, string origin, string specification, string model, int stockQuantity)
        {
            InitializeComponent();
            // 初始化控件值
            this.itemId = itemId;
            txtItemID.Text = itemId;
            txtName.Text = name;
            cmbCategory.SelectedItem = category;
            cmbOrigin.Text = origin; // 允许手动输入
            txtSpecification.Text = specification;
            txtModel.Text = model;
            numStockQuantity.Value = stockQuantity;

            // 设置只读属性
            txtItemID.ReadOnly = true;
            txtName.ReadOnly = true;
            numStockQuantity.ReadOnly = true;

            // 加载类别和产地数据
            LoadCategories();
            LoadOrigins();
        }

        private void LoadOrigins()
        {
            // 从数据库加载产地数据
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT DISTINCT origin FROM item";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbOrigin.Items.Add(reader.GetString("origin"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载产地数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadCategories()
        {
            // 从数据库加载类别数据
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT DISTINCT category FROM item";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbCategory.Items.Add(reader.GetString("category"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载类别数据时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 验证用户输入
            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("请选择物品类别！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 从 app.config 文件读取连接字符串
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "UPDATE item SET category = @Category, origin = @Origin, " +
                               "specification = @Specification, model = @Model " +
                               "WHERE item_id = @ItemID";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemID", itemId);
                    command.Parameters.AddWithValue("@Category", cmbCategory.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@Origin", cmbOrigin.Text.Trim());
                    command.Parameters.AddWithValue("@Specification", txtSpecification.Text.Trim());
                    command.Parameters.AddWithValue("@Model", txtModel.Text.Trim());

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("物品信息修改成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK; // 设置对话框结果为 OK
                        this.Close(); // 关闭窗口
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存物品信息时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否放弃当前更改？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }
            this.Close();
        }
    }
}
