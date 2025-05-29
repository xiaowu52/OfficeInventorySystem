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
using System.Threading;

namespace AdminApp
{
    public partial class StockInInfoForm : Form
    {
        public StockInInfoForm()
        {
            InitializeComponent();

            dtpStartDate.Value = DateTime.Now.AddMonths(-1); // 默认查询最近一个月  
            dtpEndDate.Value = DateTime.Now; // 默认查询到当前日期  
            LoadStockInDetails();
        }
        private void LoadStockInDetails(DateTime? startDate = null, DateTime? endDate = null)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT si.stock_in_id, si.item_id, i.name, si.purchase_date, si.quantity, si.unit_price, si.total_price  
                                    FROM stock_in si  
                                    JOIN item i ON si.item_id = i.item_id";
                if (startDate.HasValue && endDate.HasValue)
                {
                    query += " WHERE si.purchase_date BETWEEN @StartDate AND @EndDate";
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
                        DataTable stockInTable = new DataTable();
                        adapter.Fill(stockInTable);
                        dgvStockIn.DataSource = stockInTable;
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStockInDetails(dtpStartDate.Value, dtpEndDate.Value);
        }
    }
}
