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
    public partial class StockOutInfoForm : Form
    {
        public StockOutInfoForm()
        {
            InitializeComponent();
            dtpStartDate.Value = DateTime.Now.AddMonths(-1); // 默认查询最近一个月
            dtpEndDate.Value = DateTime.Now; // 默认查询到当前日期
            LoadStockOutDetails();
        }

        private void LoadStockOutDetails(DateTime? startDate = null, DateTime? endDate = null)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                   SELECT 
                       stock_out.stock_out_id, 
                       stock_out.user_name, 
                       stock_out.user_id, 
                       stock_out.request_id, 
                       request.item_id, 
                       item.name, 
                       stock_out.out_date, 
                       stock_out.quantity 
                   FROM 
                       stock_out
                   INNER JOIN 
                       request 
                   ON 
                       stock_out.request_id = request.request_id
                   INNER JOIN 
                       item 
                   ON 
                       request.item_id = item.item_id";
                if (startDate.HasValue && endDate.HasValue)
                {
                    query += " WHERE stock_out.out_date BETWEEN @StartDate AND @EndDate";
                }
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate.Value);
                        command.Parameters.AddWithValue("@EndDate", endDate.Value);
                    }
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable stockOutTable = new DataTable();
                        adapter.Fill(stockOutTable);
                        dgvStockOut.DataSource = stockOutTable;
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStockOutDetails(dtpStartDate.Value, dtpEndDate.Value);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
