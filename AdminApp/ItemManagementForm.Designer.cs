using System.Data;
using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AdminApp
{
    partial class ItemManagementForm
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
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.category = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.origin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.specification = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.model = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.image_path = new System.Windows.Forms.DataGridViewImageColumn();
            this.stock_quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddItem = new System.Windows.Forms.Button();
            this.btnDeleteItem = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.lblCurrentItem = new System.Windows.Forms.Label();
            this.btnBackToMain = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.cmbPageSize = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvItems
            // 
            this.dgvItems.AllowUserToAddRows = false;
            this.dgvItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvItems.BackgroundColor = System.Drawing.SystemColors.ActiveCaption;
            this.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.item_id,
            this.name,
            this.category,
            this.origin,
            this.specification,
            this.model,
            this.image_path,
            this.stock_quantity});
            this.dgvItems.Location = new System.Drawing.Point(-2, 93);
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.ReadOnly = true;
            this.dgvItems.RowHeadersWidth = 62;
            this.dgvItems.RowTemplate.Height = 30;
            this.dgvItems.Size = new System.Drawing.Size(1058, 724);
            this.dgvItems.TabIndex = 0;
            this.dgvItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItems_CellClick);
            this.dgvItems.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvItems_CellFormatting);
            // 
            // item_id
            // 
            this.item_id.DataPropertyName = "item_id";
            this.item_id.HeaderText = "物品编码";
            this.item_id.MinimumWidth = 8;
            this.item_id.Name = "item_id";
            this.item_id.ReadOnly = true;
            // 
            // name
            // 
            this.name.DataPropertyName = "name";
            this.name.HeaderText = "物品名称";
            this.name.MinimumWidth = 8;
            this.name.Name = "name";
            this.name.ReadOnly = true;
            // 
            // category
            // 
            this.category.DataPropertyName = "category";
            this.category.HeaderText = "物品类别";
            this.category.MinimumWidth = 8;
            this.category.Name = "category";
            this.category.ReadOnly = true;
            this.category.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // origin
            // 
            this.origin.DataPropertyName = "origin";
            this.origin.HeaderText = "产地";
            this.origin.MinimumWidth = 8;
            this.origin.Name = "origin";
            this.origin.ReadOnly = true;
            this.origin.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // specification
            // 
            this.specification.DataPropertyName = "specification";
            this.specification.HeaderText = "规格";
            this.specification.MinimumWidth = 8;
            this.specification.Name = "specification";
            this.specification.ReadOnly = true;
            // 
            // model
            // 
            this.model.DataPropertyName = "model";
            this.model.HeaderText = "型号";
            this.model.MinimumWidth = 8;
            this.model.Name = "model";
            this.model.ReadOnly = true;
            // 
            // image_path
            // 
            this.image_path.DataPropertyName = "image_path";
            this.image_path.HeaderText = "图片";
            this.image_path.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.image_path.MinimumWidth = 8;
            this.image_path.Name = "image_path";
            this.image_path.ReadOnly = true;
            this.image_path.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.image_path.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // stock_quantity
            // 
            this.stock_quantity.DataPropertyName = "stock_quantity";
            this.stock_quantity.FillWeight = 110F;
            this.stock_quantity.HeaderText = "当前库存量";
            this.stock_quantity.MinimumWidth = 8;
            this.stock_quantity.Name = "stock_quantity";
            this.stock_quantity.ReadOnly = true;
            // 
            // btnAddItem
            // 
            this.btnAddItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAddItem.Location = new System.Drawing.Point(1089, 170);
            this.btnAddItem.Name = "btnAddItem";
            this.btnAddItem.Size = new System.Drawing.Size(143, 64);
            this.btnAddItem.TabIndex = 1;
            this.btnAddItem.Text = "添加物品";
            this.btnAddItem.UseVisualStyleBackColor = true;
            this.btnAddItem.Click += new System.EventHandler(this.btnAddItem_Click);
            // 
            // btnDeleteItem
            // 
            this.btnDeleteItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteItem.Location = new System.Drawing.Point(1089, 260);
            this.btnDeleteItem.Name = "btnDeleteItem";
            this.btnDeleteItem.Size = new System.Drawing.Size(143, 68);
            this.btnDeleteItem.TabIndex = 2;
            this.btnDeleteItem.Text = "删除物品";
            this.btnDeleteItem.UseVisualStyleBackColor = true;
            this.btnDeleteItem.Click += new System.EventHandler(this.btnDeleteItem_Click);
            // 
            // btnModify
            // 
            this.btnModify.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnModify.Location = new System.Drawing.Point(1075, 350);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(172, 70);
            this.btnModify.TabIndex = 3;
            this.btnModify.Text = "修改物品信息";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // lblCurrentItem
            // 
            this.lblCurrentItem.AutoSize = true;
            this.lblCurrentItem.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrentItem.Location = new System.Drawing.Point(112, 42);
            this.lblCurrentItem.Name = "lblCurrentItem";
            this.lblCurrentItem.Size = new System.Drawing.Size(110, 31);
            this.lblCurrentItem.TabIndex = 4;
            this.lblCurrentItem.Text = "当前物品";
            // 
            // btnBackToMain
            // 
            this.btnBackToMain.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBackToMain.Location = new System.Drawing.Point(1075, 756);
            this.btnBackToMain.Name = "btnBackToMain";
            this.btnBackToMain.Size = new System.Drawing.Size(172, 61);
            this.btnBackToMain.TabIndex = 5;
            this.btnBackToMain.Text = "退出到主菜单";
            this.btnBackToMain.UseVisualStyleBackColor = true;
            this.btnBackToMain.Click += new System.EventHandler(this.btnBackToMain_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Location = new System.Drawing.Point(833, 855);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 41);
            this.btnPrevPage.TabIndex = 6;
            this.btnPrevPage.Text = "上一页";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(914, 855);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 41);
            this.btnNextPage.TabIndex = 7;
            this.btnNextPage.Text = "下一页";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(842, 834);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(17, 18);
            this.lblPageInfo.TabIndex = 8;
            this.lblPageInfo.Text = "1";
            // 
            // cmbPageSize
            // 
            this.cmbPageSize.FormattingEnabled = true;
            this.cmbPageSize.Location = new System.Drawing.Point(568, 847);
            this.cmbPageSize.Name = "cmbPageSize";
            this.cmbPageSize.Size = new System.Drawing.Size(100, 26);
            this.cmbPageSize.TabIndex = 9;
            this.cmbPageSize.SelectedIndexChanged += new System.EventHandler(this.cmbPageSize_SelectedIndexChanged);
            // 
            // ItemManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1282, 908);
            this.ControlBox = false;
            this.Controls.Add(this.cmbPageSize);
            this.Controls.Add(this.lblPageInfo);
            this.Controls.Add(this.btnNextPage);
            this.Controls.Add(this.btnPrevPage);
            this.Controls.Add(this.btnBackToMain);
            this.Controls.Add(this.lblCurrentItem);
            this.Controls.Add(this.btnModify);
            this.Controls.Add(this.btnDeleteItem);
            this.Controls.Add(this.btnAddItem);
            this.Controls.Add(this.dgvItems);
            this.Name = "ItemManagementForm";
            this.ShowInTaskbar = false;
            this.Text = "物品管理";
            this.Load += new System.EventHandler(this.ItemManagementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataGridView dgvItems;
        private Button btnAddItem;
        private Button btnDeleteItem;
        private Button btnModify;
        private Label lblCurrentItem;
        private Button btnBackToMain;
        private DataGridViewTextBoxColumn item_id;
        private DataGridViewTextBoxColumn name;
        private DataGridViewTextBoxColumn category;
        private DataGridViewTextBoxColumn origin;
        private DataGridViewTextBoxColumn specification;
        private DataGridViewTextBoxColumn model;
        private DataGridViewImageColumn image_path;
        private DataGridViewTextBoxColumn stock_quantity;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Label lblPageInfo;
        private ComboBox cmbPageSize;
    }
}