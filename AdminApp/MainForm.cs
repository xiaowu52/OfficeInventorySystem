using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // 窗体实例缓存字典（添加在类成员变量区域）
        private Dictionary<Type, Form> _childForms = new Dictionary<Type, Form>();

        private void ShowMdiChild<T>() where T : Form, new()
        {
            // 1. 检查窗体类型是否存在缓存
            if (!_childForms.TryGetValue(typeof(T), out var form) || form.IsDisposed)
            {
                form = new T();
                form.MdiParent = this; // 关键MDI设置
                form.FormClosed += (sender, e) =>
                {
                    _childForms.Remove(form.GetType());
                };
                _childForms[typeof(T)] = form;
            }

            // 2. 窗体位置重置逻辑
            if (form.WindowState == FormWindowState.Minimized)
                form.WindowState = FormWindowState.Normal;

            // 3. 激活窗体
            form.Show();
            form.BringToFront();


        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("是否退出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                // 退出应用程序
                this.Close();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)//窗体加载事件
        {
            //显示欢迎信息和管理员名称
            this.lblWelcom.Text = "管理员: admin，欢迎您";
        }

        

        private void 用户维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<UserManagementForm>();
        }

        private void 物品信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<ItemManagementForm>();
        }

        private void 物品入库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<StockInForm>();
        }

        private void 物品出库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<StockOutForm>();
        }

        private void 物品信息ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowMdiChild<ItemInfoForm>();
        }

        private void 入库明细ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<StockInInfoForm>();
        }

        private void 领用明细ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMdiChild<StockOutInfoForm>();
        }
    }
}
