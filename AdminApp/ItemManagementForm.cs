using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminApp;
using MySql.Data.MySqlClient;

namespace AdminApp
{
    public partial class ItemManagementForm : Form
    {
        private DataTable itemsTable;
        private const string CACHE_CHANNEL = "cache_invalidation";

        private int currentPage = 1;       // 当前页码
        private int pageSize = 100;        // 每页记录数
        private int totalRecords = 0;      // 总记录数
        private int totalPages = 0;        // 总页数

        public ItemManagementForm()
        {
            InitializeComponent();
            LoadItems(); // 确保在窗体初始化时加载物品数据
            // 订阅Redis通道，接收缓存失效消息
            if (RedisManager.IsAvailable)
            {
                RedisManager.SubscribeToChannel(CACHE_CHANNEL, OnCacheInvalidationMessage);
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
                LoadItems();
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            AddItemForm addItemForm = new AddItemForm();
            // 打开窗体  
            addItemForm.ShowDialog();

            // 使用新的Redis缓存失效方法
            RedisManager.InvalidateItemsCache();
            // 通知其他应用程序缓存已更新
            RedisManager.PublishMessage(CACHE_CHANNEL, "items_updated");
            
            LoadItems(); // 添加物品后刷新数据 
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            // 检查是否选中单元格
            if (dgvItems.CurrentCell != null)
            {
                int selectedIndex = dgvItems.CurrentCell.RowIndex; // 获取当前单元格所在的行索引
                string item_id = dgvItems.Rows[selectedIndex].Cells["item_id"].Value.ToString();
                //确认删除提示，提示lblCurrentItem的值
                if(MessageBox.Show($"是否删除物品：{lblCurrentItem.Text}？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)== DialogResult.No)
                {
                    return;
                }
                // 从 app.config 文件读取连接字符串
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM item WHERE item_id = @item_id";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@item_id", item_id);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    MessageBox.Show("物品删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                // 使用Redis缓存失效方法
                RedisManager.InvalidateItemsCache();
                // 通知其他应用程序缓存已更新
                RedisManager.PublishMessage(CACHE_CHANNEL, "items_updated");
                
                // 刷新数据
                LoadItems();
            }
            else
            {
                MessageBox.Show("请先选择要删除的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadItems(int page = 1, int size = 100)
        {
            // 更新当前页码和页大小
            currentPage = page;
            pageSize = size;

            // 尝试从缓存获取
            string cacheKey = $"{RedisManager.ITEM_CACHE_KEY}_page_{page}_size_{size}";
            if (RedisManager.IsAvailable)
            {
                var cachedTable = RedisManager.GetDataTable(cacheKey);
                if (cachedTable != null)
                {
                    itemsTable = cachedTable;
                    dgvItems.DataSource = itemsTable;

                    // 更新分页信息（从缓存获取总记录数）
                    string countKey = $"{RedisManager.ITEM_CACHE_KEY}_count";
                    var cachedCount = RedisManager.GetValue(countKey);
                    if (!string.IsNullOrEmpty(cachedCount))
                    {
                        totalRecords = int.Parse(cachedCount);
                        UpdatePagingControls();
                    }
                    else
                    {
                        // 如果没有缓存的总数，则需要查询一次
                        FetchTotalRecords();
                    }

                    return;
                }
            }

            // 从数据库加载数据
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 1. 获取总记录数
                FetchTotalRecords(connection);

                // 2. 查询当前页的数据
                string query = "SELECT * FROM item ORDER BY item_id LIMIT @Offset, @PageSize";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Offset", (currentPage - 1) * pageSize);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        itemsTable = new DataTable();
                        adapter.Fill(itemsTable);
                        dgvItems.DataSource = itemsTable;

                        // 3. 将结果存入缓存
                        if (RedisManager.IsAvailable)
                        {
                            RedisManager.SetDataTable(cacheKey, itemsTable, TimeSpan.FromMinutes(5));
                        }
                    }
                }
            }

            // 更新分页控件显示
            UpdatePagingControls();
        }

        // 获取总记录数的方法
        private void FetchTotalRecords(MySqlConnection existingConnection = null)
        {
            bool needToCloseConnection = false;
            MySqlConnection connection = existingConnection;

            try
            {
                if (connection == null)
                {
                    // 如果没有传入连接，则创建新的连接
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    needToCloseConnection = true;
                }

                string countQuery = "SELECT COUNT(*) FROM item";
                using (MySqlCommand command = new MySqlCommand(countQuery, connection))
                {
                    totalRecords = Convert.ToInt32(command.ExecuteScalar());
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                    // 缓存总记录数
                    if (RedisManager.IsAvailable)
                    {
                        RedisManager.SetValue($"{RedisManager.ITEM_CACHE_KEY}_count", totalRecords.ToString(), TimeSpan.FromMinutes(30));
                    }
                }
            }
            finally
            {
                // 如果是此方法创建的连接，则需要关闭
                if (needToCloseConnection && connection != null)
                {
                    connection.Close();
                }
            }
        }

        // 更新分页控件显示
        private void UpdatePagingControls()
        {
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // 更新页码信息显示
            lblPageInfo.Text = $"第 {currentPage} 页，共 {totalPages} 页 (总计 {totalRecords} 条记录)";

            // 启用或禁用翻页按钮
            btnPrevPage.Enabled = (currentPage > 1);
            btnNextPage.Enabled = (currentPage < totalPages);
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            // 检查是否选中了一行
            if (dgvItems.CurrentRow != null)
            {
                // 获取选中行的数据
                DataGridViewRow selectedRow = dgvItems.CurrentRow;
                string itemId = selectedRow.Cells["item_id"].Value.ToString();
                string name = selectedRow.Cells["name"].Value.ToString();
                string category = selectedRow.Cells["category"].Value.ToString();
                string origin = selectedRow.Cells["origin"].Value?.ToString();
                string specification = selectedRow.Cells["specification"].Value?.ToString();
                string model = selectedRow.Cells["model"].Value?.ToString();
                int stockQuantity = Convert.ToInt32(selectedRow.Cells["stock_quantity"].Value);

                // 打开 ModifyItemForm
                ModifyItemForm modifyItemForm = new ModifyItemForm(itemId, name, category, origin, specification, model, stockQuantity);
                if (modifyItemForm.ShowDialog() == DialogResult.OK)
                {
                    // 使用新的Redis缓存失效方法
                    RedisManager.InvalidateItemsCache();
                    // 通知其他应用程序缓存已更新
                    RedisManager.PublishMessage(CACHE_CHANNEL, "items_updated");
                    
                    // 如果修改成功，刷新 DataGridView
                    LoadItems();
                }
            }
            else
            {
                MessageBox.Show("请先选择要修改的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBackToMain_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 添加一个方法来计算 DataGridView 可容纳的行数
        private int CalculatePageSizeBasedOnGridHeight()
        {
            // 获取可见区域高度（排除标题行）
            int visibleHeight = dgvItems.Height - dgvItems.ColumnHeadersHeight;

            // 如果没有行，无法计算行高，返回默认值
            if (dgvItems.Rows.Count == 0)
                return 100; // 默认页大小

            // 获取行高 (假设所有行高度一致)
            int rowHeight = dgvItems.Rows[0].Height;

            // 计算可显示的行数 (留出一点空间防止出现垂直滚动条)
            int visibleRows = (visibleHeight / rowHeight);

            // 确保返回合理的数值
            if (visibleRows <= 0)
                visibleRows = 10; // 最小显示10行

            // 可以将结果舍入到最接近的整数倍数，使分页更规整
            int roundTo = 10; // 例如舍入到10的倍数
            return (int)Math.Ceiling(visibleRows / (double)roundTo) * roundTo;
        }

        private void ItemManagementForm_Load(object sender, EventArgs e)
        {
            dgvItems.AutoGenerateColumns = false;

            // 初始化页大小下拉框
            cmbPageSize.Items.AddRange(new object[] { "自动", "50", "100", "200", "500" });
            cmbPageSize.SelectedIndex = 0; // 默认选择"自动"

            // 根据当前 DataGridView 大小计算合适的页大小
            if (cmbPageSize.SelectedIndex == 0) // 如果选择了"自动"
            {
                pageSize = CalculatePageSizeBasedOnGridHeight();
            }

            LoadItems(1, pageSize); // 加载第一页数据

        }

        private void dgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //选中行
            if (e.RowIndex >= 0 && e.RowIndex < dgvItems.Rows.Count)
            {
                //获取选中行
                DataGridViewRow selectedRow = dgvItems.Rows[e.RowIndex];
                //获取选中行的单元格
                foreach (DataGridViewCell cell in selectedRow.Cells)
                {
                    //判断单元格是否可编辑
                    if (cell.ReadOnly == false)
                    {
                        cell.Selected = true;
                    }
                }
            }
            //lblCurrentItem显示当前物品名字
            lblCurrentItem.Text = "当前物品：" + dgvItems.Rows[e.RowIndex].Cells["name"].Value.ToString();
        }

        private void dgvItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvItems.Columns[e.ColumnIndex].Name == "image_path" && e.Value != null)
            {
                string path = e.Value.ToString();

                try
                {
                    if (File.Exists(path))
                    {
                        // 异步加载避免界面卡顿
                        var task = Task.Run(() => Image.FromFile(path));
                        e.Value = task.Result;
                    }
                    else
                    {
                        e.Value = Properties.Resources.DefaultImage; // 备用图片
                    }
                }
                catch
                {
                    e.Value = Properties.Resources.ErrorImage;
                }
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                LoadItems(currentPage - 1, pageSize);
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                LoadItems(currentPage + 1, pageSize);
            }
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 处理选择的页码大小
            if (cmbPageSize.SelectedIndex == 0) // "自动"选项
            {
                pageSize = CalculatePageSizeBasedOnGridHeight();
            }
            else if (int.TryParse(cmbPageSize.SelectedItem.ToString(), out int selectedSize))
            {
                pageSize = selectedSize;
            }

            // 切换页大小时回到第一页
            currentPage = 1;
            LoadItems(currentPage, pageSize);
        }
    }



}
