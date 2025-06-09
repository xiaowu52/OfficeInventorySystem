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
    public partial class StockInForm : Form
    {
        public StockInForm()
        {
            InitializeComponent();
        }
        private void CalculateTotal()
        {
            if (decimal.TryParse(txtUnitPrice.Text, out decimal price)
                && numQuantity.Value > 0)
            {
                txtTotalPrice.Text = (price * numQuantity.Value).ToString("N2");
            }
            else
            {
                txtTotalPrice.Text = "0.00";
            }
        }
        private void txtUnitPrice_TextChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }
        private void numQuantity_ValueChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }
        private List<ComboBoxItem> LoadItemList(int pageSize = 100, int pageNumber = 1)
        {
            var list = new List<ComboBoxItem>();
            // 从 app.config 文件读取连接字符串
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // 使用LIMIT和OFFSET实现分页查询
                var cmd = new MySqlCommand(
                    "SELECT item_id, name FROM item ORDER BY item_id LIMIT @pageSize OFFSET @offset", conn);

                cmd.Parameters.AddWithValue("@pageSize", pageSize);
                cmd.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ComboBoxItem(
                            reader["name"].ToString(),
                            reader["item_id"].ToString()
                        ));
                    }
                }
            }
            return list;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            //物品编码下拉框，下拉框内容为数据库中已经存在的物品编码
            if (cboItemId.SelectedItem == null)
            {
                MessageBox.Show("请选择物品编码");
                return;
            }
            

            if (dtpPurchase.Value > DateTime.Today.AddDays(1))
            {
                MessageBox.Show("购买日期不能超过今日");
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price < 0.01m)
            {
                MessageBox.Show("单价必须大于等于0.01元");
                return;
            }

            if (numQuantity.Value < 1 || numQuantity.Value > 10000)
            {
                MessageBox.Show("数量需在1-10000之间");
                return;
            }

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 插入入库记录
                        var cmd = new MySqlCommand(@"
                        INSERT INTO stock_in 
                            (item_id, purchase_date, quantity, unit_price) 
                        VALUES 
                            (@item_id, @date, @qty, @price)", conn);

                        cmd.Parameters.AddWithValue("@item_id", ((ComboBoxItem)cboItemId.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@date", dtpPurchase.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@qty", (int)numQuantity.Value);
                        cmd.Parameters.AddWithValue("@price", price);

                        cmd.ExecuteNonQuery();

                        // 提交事务
                        transaction.Commit();
                        MessageBox.Show("入库成功！库存已更新");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        MessageBox.Show($"操作失败：{ex.Message}");
                    }
                }
            }
            this.Close();
        }
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public ComboBoxItem(string text, string value)
            {
                Text = text;
                Value = value;
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
           if(MessageBox.Show("是否放弃当前更改？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }
            this.Close();
        }

        private void StockInForm_Load(object sender, EventArgs e)
        {
            cboItemId.Enabled = false;
            try
            {
                var items = LoadItemList();
                BindComboBox(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载物品列表失败：{ex.Message}");
            }
            finally
            {
                cboItemId.Enabled = true;
            }
        }

        // 在StockInForm.cs中添加一个方法来获取物品的平均价格
        private decimal GetAveragePrice(string itemId)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            decimal avgPrice = 0;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // 从视图中查询平均价格
                    var cmd = new MySqlCommand(@"
                        SELECT average_price 
                        FROM item_avg_price_view
                        WHERE item_id = @item_id", conn);

                    cmd.Parameters.AddWithValue("@item_id", itemId);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        avgPrice = Convert.ToDecimal(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取平均价格失败: {ex.Message}");
            }

            return avgPrice;
        }

        // 更新显示均价的标签
        private void UpdateAveragePriceLabel(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                lblAveragePrice.Text = "";
                return;
            }

            decimal avgPrice = GetAveragePrice(itemId);

            if (avgPrice > 0)
            {
                lblAveragePrice.Text = $"(历史均价: ¥{avgPrice:N2})";
            }
            else
            {
                lblAveragePrice.Text = "(无历史价格)";
            }
        }
        private void BindComboBox(List<ComboBoxItem> items)
        {
            cboItemId.BeginUpdate();  // 批量更新优化性能
            cboItemId.Items.Clear();
            cboItemId.Items.AddRange(items.ToArray());
            cboItemId.DisplayMember = "Text";
            cboItemId.ValueMember = "Value";
            cboItemId.EndUpdate();
        }

        private void cboItemId_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboItemId.SelectedItem != null)
            {
                string selectedItemId = ((ComboBoxItem)cboItemId.SelectedItem).Value;
                UpdateAveragePriceLabel(selectedItemId);
            }
            else
            {
                lblAveragePrice.Text = "";
            }
        }
    }
}
