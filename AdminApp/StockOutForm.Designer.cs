using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace AdminApp
{
    partial class StockOutForm
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvRequests = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.request_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.user_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.user_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.request_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvRequests
            // 
            this.dgvRequests.AllowUserToAddRows = false;
            this.dgvRequests.AllowUserToDeleteRows = false;
            this.dgvRequests.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRequests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRequests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.request_id,
            this.item_id,
            this.user_id,
            this.user_name,
            this.request_date,
            this.quantity,
            this.status});
            this.dgvRequests.Location = new System.Drawing.Point(2, 151);
            this.dgvRequests.Name = "dgvRequests";
            this.dgvRequests.RowHeadersWidth = 62;
            this.dgvRequests.RowTemplate.Height = 30;
            this.dgvRequests.Size = new System.Drawing.Size(1107, 773);
            this.dgvRequests.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnRefresh.Location = new System.Drawing.Point(1080, 65);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(186, 49);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷新当前列表";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConfirm.Location = new System.Drawing.Point(1146, 268);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(137, 56);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "确认通过";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnReject
            // 
            this.btnReject.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnReject.Location = new System.Drawing.Point(1146, 363);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(137, 48);
            this.btnReject.TabIndex = 4;
            this.btnReject.Text = "驳回";
            this.btnReject.UseVisualStyleBackColor = true;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);
            // 
            // request_id
            // 
            this.request_id.DataPropertyName = "request_id";
            this.request_id.HeaderText = "申请号";
            this.request_id.MinimumWidth = 8;
            this.request_id.Name = "request_id";
            // 
            // item_id
            // 
            this.item_id.DataPropertyName = "item_id";
            this.item_id.HeaderText = "物品编码";
            this.item_id.MinimumWidth = 8;
            this.item_id.Name = "item_id";
            // 
            // user_id
            // 
            this.user_id.DataPropertyName = "user_id";
            this.user_id.HeaderText = "申请人ID";
            this.user_id.MinimumWidth = 8;
            this.user_id.Name = "user_id";
            // 
            // user_name
            // 
            this.user_name.DataPropertyName = "user_name";
            this.user_name.HeaderText = "申请人姓名";
            this.user_name.MinimumWidth = 8;
            this.user_name.Name = "user_name";
            // 
            // request_date
            // 
            this.request_date.DataPropertyName = "request_date";
            this.request_date.HeaderText = "出库日期";
            this.request_date.MinimumWidth = 8;
            this.request_date.Name = "request_date";
            // 
            // quantity
            // 
            this.quantity.DataPropertyName = "quantity";
            this.quantity.HeaderText = "出库量";
            this.quantity.MinimumWidth = 8;
            this.quantity.Name = "quantity";
            // 
            // status
            // 
            this.status.DataPropertyName = "status";
            this.status.HeaderText = "申请状态";
            this.status.MinimumWidth = 8;
            this.status.Name = "status";
            this.status.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // StockOutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1337, 925);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.dgvRequests);
            this.Name = "StockOutForm";
            this.Text = "物品出库";
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView dgvRequests;
        private Button btnRefresh;
        private Button btnConfirm;
        private Button btnReject;
        private DataGridViewTextBoxColumn request_id;
        private DataGridViewTextBoxColumn item_id;
        private DataGridViewTextBoxColumn user_id;
        private DataGridViewTextBoxColumn user_name;
        private DataGridViewTextBoxColumn request_date;
        private DataGridViewTextBoxColumn quantity;
        private DataGridViewTextBoxColumn status;
    }
}