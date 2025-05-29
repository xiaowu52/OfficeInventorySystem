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
    public partial class ItemInfoForm : Form
    {
        // 添加一个标志用于跟踪是否正在加载数据
        private bool isLoading = false;
        // 当前页码
        private int currentPage = 1;
        // 每页显示的记录数
        private int pageSize = 100;
        public ItemInfoForm()
        {
            InitializeComponent();

            // 窗体显示后异步加载数据
            this.Shown += async (s, e) => await LoadStockInfoAsync();

        }

        private async Task LoadStockInfoAsync(string searchKeyword = "")
        {
            if (isLoading) return;

            try
            {
                isLoading = true;
                lblLoading.Visible = true;
                dgvStockInfo.Enabled = false;

                // 更新页码显示
                lblPageInfo.Text = $"第{currentPage}页";
                btnPrevPage.Enabled = currentPage > 1;

                // 使用Task.Run在后台线程执行数据库查询
                var stockTable = await Task.Run(() => FetchStockDataFromDb(searchKeyword, pageSize, currentPage));

                // 在UI线程更新DataGridView
                dgvStockInfo.DataSource = stockTable;

                // 如果返回的记录数小于页大小，禁用下一页按钮
                btnNextPage.Enabled = stockTable.Rows.Count >= pageSize;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据时发生错误: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                lblLoading.Visible = false;
                dgvStockInfo.Enabled = true;
            }
        }

        private DataTable FetchStockDataFromDb(string searchKeyword = "", int pageSize = 100, int pageNumber = 1)
        {
            DataTable stockTable = new DataTable();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT item_id, name, category, stock_quantity FROM item";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    query += " WHERE name LIKE @Search OR item_id LIKE @Search";
                }
                query += " ORDER BY item_id LIMIT @pageSize OFFSET @offset";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                    }
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(stockTable);
                    }
                }
            }

            return stockTable;
        }

        private async Task ChangePage(int direction)
        {
            if (direction < 0 && currentPage > 1)
            {
                currentPage--;
            }
            else if (direction > 0)
            {
                currentPage++;
            }

            await LoadStockInfoAsync(txtSearch.Text.Trim());
        }

        private async Task SearchItemsAsync()
        {
            // 搜索时重置到第一页
            currentPage = 1;
            await LoadStockInfoAsync(txtSearch.Text.Trim());
        }

        private void LoadStockInfo(string searchKeyword = "", int pageSize = 100, int pageNumber = 1)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT item_id, name, category, stock_quantity FROM item";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    query += " WHERE name LIKE @Search OR item_id LIKE @Search";
                }
                query += " ORDER BY item_id LIMIT @pageSize OFFSET @offset";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                    }
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable stockTable = new DataTable();
                        adapter.Fill(stockTable);
                        dgvStockInfo.DataSource = stockTable;
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            _= SearchItemsAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
