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

namespace AdminApp
{
    public partial class AddItemForm : Form
    {
        public AddItemForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //判断是否有未保存的更改
            if (MessageBox.Show("是否放弃当前更改？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }
            this.Close();
        }

        private void btnUploadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 显示图片
                    pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                    pictureBox.Tag = openFileDialog.FileName; // 将图片路径存储到 Tag 属性
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 验证用户输入
            if (string.IsNullOrWhiteSpace(txtItemID.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("物品编码和物品名称不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("请选择物品类别！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取图片路径
            string imagePath = pictureBox.Tag?.ToString();

            // 保存到数据库

            // 从 app.config 文件读取连接字符串
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO item (item_id, name, category, origin, specification, model, image_path) " +
                               "VALUES (@ItemID, @Name, @Category, @Origin, @Specification, @Model, @ImagePath)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemID", txtItemID.Text.Trim());
                    command.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                    command.Parameters.AddWithValue("@Category", cmbCategory.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@Origin", cmbOrigin.Text.Trim());
                    command.Parameters.AddWithValue("@Specification", txtSpecification.Text.Trim());
                    command.Parameters.AddWithValue("@Model", txtModel.Text.Trim());
                    command.Parameters.AddWithValue("@ImagePath", imagePath);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show("物品信息保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // 关闭窗口
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存物品信息时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
