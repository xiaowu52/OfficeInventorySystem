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
    public partial class StockOutForm : Form
    {
        public StockOutForm()
        {
            InitializeComponent();
            LoadPendingRequests();
        }
        private void LoadPendingRequests()// 加载待处理申请（包含库存信息）
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT request_id, item_id, user_id, user_name, request_date, quantity, status " +
                               "FROM request WHERE status = '申请中'";
                var adapter = new MySqlDataAdapter(query, conn);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgvRequests.DataSource = dt;
            }
        }
        private void ConfirmRequest(int requestId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 步骤1：验证库存
                        var checkCmd = new MySqlCommand(
                            "SELECT stock_quantity FROM item WHERE item_id = " +
                            "(SELECT item_id FROM request WHERE request_id = @reqId)", conn);
                        checkCmd.Parameters.AddWithValue("@reqId", requestId);
                        var currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());

                        var requestQty = GetRequestQuantity(requestId); // 获取申请数量
                        if (currentStock < requestQty)
                        {
                            throw new Exception("库存不足，当前库存：" + currentStock);
                        }

                        // 步骤2：更新申请状态（触发数据库触发器）
                        var updateCmd = new MySqlCommand(
                            "UPDATE request SET status = '已通过' WHERE request_id = @reqId",
                            conn, transaction);
                        updateCmd.Parameters.AddWithValue("@reqId", requestId);
                        updateCmd.ExecuteNonQuery();

                       
                        // 步骤3：更新出库表
                        var insertCmd = new MySqlCommand(
                            "INSERT INTO stock_out (user_id, user_name, request_id, out_date, quantity) " +
                            "SELECT user_id, user_name, request_id, NOW(), quantity FROM request WHERE request_id = @reqId",
                            conn, transaction);
                        insertCmd.Parameters.AddWithValue("@reqId", requestId);
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                        MessageBox.Show("出库成功！");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"操作失败：{ex.Message}");
                    }
                }
            }
        }
        private void RejectRequest(int requestId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE request SET status = '已驳回'" +
                    "WHERE request_id = @reqId", conn);
                cmd.Parameters.AddWithValue("@reqId", requestId);
                cmd.ExecuteNonQuery();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // 刷新数据
            LoadPendingRequests();
        }
        private int GetRequestQuantity(int requestId)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT quantity FROM request WHERE request_id = @reqId", conn);
                cmd.Parameters.AddWithValue("@reqId", requestId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // 确认出库
            if (dgvRequests.CurrentCell != null)
            {
                int selectedIndex = dgvRequests.CurrentCell.RowIndex; // 获取当前单元格所在的行索引
                int requestId = Convert.ToInt32(dgvRequests.Rows[selectedIndex].Cells["request_id"].Value);
                ConfirmRequest(requestId);
                LoadPendingRequests();
            }
            else
            {
                MessageBox.Show("请先选择一条申请记录。");
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            // 驳回申请
            if (dgvRequests.CurrentCell != null)
            {
                int selectedIndex = dgvRequests.CurrentCell.RowIndex; // 获取当前单元格所在的行索引
                int requestId = Convert.ToInt32(dgvRequests.Rows[selectedIndex].Cells["request_id"].Value);
                RejectRequest(requestId);
                LoadPendingRequests();
            }
            else
            {
                MessageBox.Show("请先选择一条申请记录。");
            }
        }
    }
}
