using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                
                // 使用新的Redis缓存失效方法
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

        private void LoadItems()
        {
            // 尝试从缓存获取，使用共享的缓存键
            if (RedisManager.IsAvailable)
            {
                var cachedTable = RedisManager.GetDataTable(RedisManager.ITEM_CACHE_KEY);
                if (cachedTable != null)
                {
                    itemsTable = cachedTable;
                    dgvItems.DataSource = itemsTable;
                    return; // 使用缓存数据，直接返回
                }
            }

            // 从 app.config 文件读取连接字符串  
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM item";
                //读取数据库数据到dgv
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                {
                    itemsTable = new DataTable();
                    adapter.Fill(itemsTable);
                    dgvItems.DataSource = itemsTable;

                    // 将结果存入缓存，使用共享的缓存键
                    if (RedisManager.IsAvailable)
                    {
                        RedisManager.SetDataTable(RedisManager.ITEM_CACHE_KEY, itemsTable, TimeSpan.FromMinutes(10));
                    }
                }
            }
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
        private void ItemManagementForm_Load(object sender, EventArgs e)
        {
            dgvItems.AutoGenerateColumns = false;
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
    }



}
