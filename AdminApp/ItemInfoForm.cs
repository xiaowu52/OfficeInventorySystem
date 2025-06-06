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
        // 分页相关变量
        private int currentPage = 1;
        private int pageSize = 100;
        private int totalRecords = 0;
        private int totalPages = 0;
        private bool isLoading = false;

        // 缓存相关常量
        private const string CACHE_CHANNEL = "cache_invalidation";
        private const string ITEM_INFO_CACHE_PREFIX = "item_info_view_";

        public ItemInfoForm()
        {
            InitializeComponent();

            // 窗体加载完成事件
            this.Load += ItemInfoForm_Load;

            // 订阅Redis通道，接收缓存失效消息
            if (RedisManager.IsAvailable)
            {
                RedisManager.SubscribeToChannel(CACHE_CHANNEL, OnCacheInvalidationMessage);
            }
        }

        private void ItemInfoForm_Load(object sender, EventArgs e)
        {
            // 设置DataGridView属性
            SetupDataGridView();

            // 初次加载数据
            LoadItemInfoAsync().ConfigureAwait(false);
        }

        private void SetupDataGridView()
        {
            // 配置DataGridView属性
            dgvStockInfo.AutoGenerateColumns = false;

            // 如果列不存在，则创建列
            if (dgvStockInfo.Columns.Count == 0)
            {
                // 添加物品ID列
                DataGridViewTextBoxColumn itemIdColumn = new DataGridViewTextBoxColumn();
                itemIdColumn.DataPropertyName = "item_id";
                itemIdColumn.HeaderText = "物品编码";
                itemIdColumn.Width = 150;
                dgvStockInfo.Columns.Add(itemIdColumn);

                // 添加物品名称列
                DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
                nameColumn.DataPropertyName = "name";
                nameColumn.HeaderText = "物品名称";
                nameColumn.Width = 200;
                dgvStockInfo.Columns.Add(nameColumn);

                // 添加物品类别列
                DataGridViewTextBoxColumn categoryColumn = new DataGridViewTextBoxColumn();
                categoryColumn.DataPropertyName = "category";
                categoryColumn.HeaderText = "物品类别";
                categoryColumn.Width = 150;
                dgvStockInfo.Columns.Add(categoryColumn);

                // 添加产量列（stock_quantity）
                DataGridViewTextBoxColumn yieldColumn = new DataGridViewTextBoxColumn();
                yieldColumn.DataPropertyName = "yield";
                yieldColumn.HeaderText = "库存数量";
                yieldColumn.Width = 150;
                dgvStockInfo.Columns.Add(yieldColumn);
            }
        }

        private void OnCacheInvalidationMessage(string message)
        {
            // 在UI线程上处理消息
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ProcessCacheMessage(message)));
            }
            else
            {
                ProcessCacheMessage(message);
            }
        }

        private void ProcessCacheMessage(string message)
        {
            // 根据消息类型刷新相应数据
            if (message == "items_updated")
            {
                LoadItemInfoAsync().ConfigureAwait(false);
            }
        }

        private async Task LoadItemInfoAsync()
        {
            if (isLoading) return;

            try
            {
                isLoading = true;

                // 显示加载状态
                lblLoading.Visible = true;
                dgvStockInfo.Enabled = false;

                // 1. 获取总记录数
                totalRecords = await Task.Run(() => GetTotalRecords(txtSearch.Text.Trim()));

                // 2. 计算总页数
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // 3. 尝试从缓存获取分页数据
                string cacheKey = $"{ITEM_INFO_CACHE_PREFIX}page_{currentPage}_size_{pageSize}";
                if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
                {
                    cacheKey += $"_search_{txtSearch.Text.Trim()}";
                }

                DataTable itemsTable = null;
                if (RedisManager.IsAvailable)
                {
                    itemsTable = RedisManager.GetDataTable(cacheKey);
                }

                // 4. 如果缓存中没有，则从数据库加载
                if (itemsTable == null)
                {
                    itemsTable = await Task.Run(() => FetchItemInfoFromView(txtSearch.Text.Trim(), pageSize, currentPage));

                    // 缓存查询结果
                    if (RedisManager.IsAvailable && itemsTable != null && itemsTable.Rows.Count > 0)
                    {
                        RedisManager.SetDataTable(cacheKey, itemsTable, TimeSpan.FromMinutes(5));
                    }
                }

                // 5. 更新UI
                dgvStockInfo.DataSource = itemsTable;

                // 6. 更新分页信息和控制按钮状态
                UpdatePagingInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载物品信息失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                lblLoading.Visible = false;
                dgvStockInfo.Enabled = true;
            }
        }

        private int GetTotalRecords(string searchKeyword = "")
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string countQuery = "SELECT COUNT(*) FROM v_item_info";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    countQuery += " WHERE name LIKE @Search OR item_id LIKE @Search";
                }

                using (MySqlCommand command = new MySqlCommand(countQuery, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                    }

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private DataTable FetchItemInfoFromView(string searchKeyword = "", int pageSize = 100, int pageNumber = 1)
        {
            DataTable itemsTable = new DataTable();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 使用视图查询数据
                string query = "SELECT item_id, name, category, yield FROM v_item_info";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    query += " WHERE name LIKE @Search OR item_id LIKE @Search";
                }
                query += " ORDER BY item_id LIMIT @PageSize OFFSET @Offset";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");
                    }
                    command.Parameters.AddWithValue("@PageSize", pageSize);
                    command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(itemsTable);
                    }
                }
            }

            return itemsTable;
        }

        private void UpdatePagingInfo()
        {
            // 更新页码显示
            lblPageInfo.Text = $"第 {currentPage} / {totalPages} 页 (共 {totalRecords} 条记录)";

            // 启用或禁用翻页按钮
            btnPrevPage.Enabled = (currentPage > 1);
            btnNextPage.Enabled = (currentPage < totalPages);
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // 搜索时重置到第一页
            currentPage = 1;
            await LoadItemInfoAsync();
        }

        private async void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                await LoadItemInfoAsync();
            }
        }

        private async void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                await LoadItemInfoAsync();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
