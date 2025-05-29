using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Windows.Forms;

namespace AdminApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.用户维护ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.物品信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.物品入库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.物品出库ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.库存查询ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.物品信息ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.入库明细ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.领用明细ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblWelcom = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(35, 35);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.用户维护ToolStripMenuItem,
            this.物品信息ToolStripMenuItem,
            this.物品入库ToolStripMenuItem,
            this.物品出库ToolStripMenuItem,
            this.库存查询ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1296, 45);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 用户维护ToolStripMenuItem
            // 
            this.用户维护ToolStripMenuItem.Name = "用户维护ToolStripMenuItem";
            this.用户维护ToolStripMenuItem.Size = new System.Drawing.Size(145, 41);
            this.用户维护ToolStripMenuItem.Text = "用户维护";
            this.用户维护ToolStripMenuItem.Click += new System.EventHandler(this.用户维护ToolStripMenuItem_Click);
            // 
            // 物品信息ToolStripMenuItem
            // 
            this.物品信息ToolStripMenuItem.Name = "物品信息ToolStripMenuItem";
            this.物品信息ToolStripMenuItem.Size = new System.Drawing.Size(145, 41);
            this.物品信息ToolStripMenuItem.Text = "物品信息";
            this.物品信息ToolStripMenuItem.Click += new System.EventHandler(this.物品信息ToolStripMenuItem_Click);
            // 
            // 物品入库ToolStripMenuItem
            // 
            this.物品入库ToolStripMenuItem.Name = "物品入库ToolStripMenuItem";
            this.物品入库ToolStripMenuItem.Size = new System.Drawing.Size(145, 41);
            this.物品入库ToolStripMenuItem.Text = "物品入库";
            this.物品入库ToolStripMenuItem.Click += new System.EventHandler(this.物品入库ToolStripMenuItem_Click);
            // 
            // 物品出库ToolStripMenuItem
            // 
            this.物品出库ToolStripMenuItem.Name = "物品出库ToolStripMenuItem";
            this.物品出库ToolStripMenuItem.Size = new System.Drawing.Size(145, 41);
            this.物品出库ToolStripMenuItem.Text = "物品出库";
            this.物品出库ToolStripMenuItem.Click += new System.EventHandler(this.物品出库ToolStripMenuItem_Click);
            // 
            // 库存查询ToolStripMenuItem
            // 
            this.库存查询ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.物品信息ToolStripMenuItem1,
            this.入库明细ToolStripMenuItem,
            this.领用明细ToolStripMenuItem});
            this.库存查询ToolStripMenuItem.Name = "库存查询ToolStripMenuItem";
            this.库存查询ToolStripMenuItem.Size = new System.Drawing.Size(145, 41);
            this.库存查询ToolStripMenuItem.Text = "库存查询";
            // 
            // 物品信息ToolStripMenuItem1
            // 
            this.物品信息ToolStripMenuItem1.Name = "物品信息ToolStripMenuItem1";
            this.物品信息ToolStripMenuItem1.Size = new System.Drawing.Size(236, 46);
            this.物品信息ToolStripMenuItem1.Text = "物品信息";
            this.物品信息ToolStripMenuItem1.Click += new System.EventHandler(this.物品信息ToolStripMenuItem1_Click);
            // 
            // 入库明细ToolStripMenuItem
            // 
            this.入库明细ToolStripMenuItem.Name = "入库明细ToolStripMenuItem";
            this.入库明细ToolStripMenuItem.Size = new System.Drawing.Size(236, 46);
            this.入库明细ToolStripMenuItem.Text = "入库明细";
            this.入库明细ToolStripMenuItem.Click += new System.EventHandler(this.入库明细ToolStripMenuItem_Click);
            // 
            // 领用明细ToolStripMenuItem
            // 
            this.领用明细ToolStripMenuItem.Name = "领用明细ToolStripMenuItem";
            this.领用明细ToolStripMenuItem.Size = new System.Drawing.Size(236, 46);
            this.领用明细ToolStripMenuItem.Text = "领用明细";
            this.领用明细ToolStripMenuItem.Click += new System.EventHandler(this.领用明细ToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1191, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 18);
            this.label2.TabIndex = 8;
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExit.ForeColor = System.Drawing.Color.IndianRed;
            this.btnExit.Location = new System.Drawing.Point(1133, 0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(152, 47);
            this.btnExit.TabIndex = 9;
            this.btnExit.Text = "退出登录";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblWelcom
            // 
            this.lblWelcom.AutoSize = true;
            this.lblWelcom.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblWelcom.Location = new System.Drawing.Point(779, 9);
            this.lblWelcom.Name = "lblWelcom";
            this.lblWelcom.Size = new System.Drawing.Size(110, 31);
            this.lblWelcom.TabIndex = 12;
            this.lblWelcom.Text = "欢迎信息";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1296, 857);
            this.Controls.Add(this.lblWelcom);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "管理员主界面";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem 用户维护ToolStripMenuItem;
        private ToolStripMenuItem 物品信息ToolStripMenuItem;
        private ToolStripMenuItem 物品入库ToolStripMenuItem;
        private ToolStripMenuItem 物品出库ToolStripMenuItem;
        private ToolStripMenuItem 库存查询ToolStripMenuItem;
        private Label label2;
        private Button btnExit;
        private Label lblWelcom;
        private ToolStripMenuItem 物品信息ToolStripMenuItem1;
        private ToolStripMenuItem 入库明细ToolStripMenuItem;
        private ToolStripMenuItem 领用明细ToolStripMenuItem;
    }
}