using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO; // 添加此命名空间以解决“File”未定义的问题


namespace AdminApp
{
    partial class UserManagementForm
    {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.UserID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Gender = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.BirthDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhoneNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblCurrentuser = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.btnAdduser = new System.Windows.Forms.Button();
            this.btnDeleteuser = new System.Windows.Forms.Button();
            this.btnModifyconfirm = new System.Windows.Forms.Button();
            this.btnBackToMain = new System.Windows.Forms.Button();
            this.userBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.userBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.BackgroundColor = System.Drawing.SystemColors.Info;
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserID,
            this.Password,
            this.UserName,
            this.Gender,
            this.BirthDate,
            this.PhoneNumber});
            this.dgvUsers.Location = new System.Drawing.Point(-1, 102);
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.RowHeadersWidth = 62;
            this.dgvUsers.RowTemplate.Height = 30;
            this.dgvUsers.Size = new System.Drawing.Size(1391, 894);
            this.dgvUsers.TabIndex = 0;
            this.dgvUsers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellClick);
            this.dgvUsers.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellValidated);
            this.dgvUsers.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgv_CellValidating);
            this.dgvUsers.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellValueChanged);
            this.dgvUsers.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_RowHeaderMouseClick);
            // 
            // UserID
            // 
            this.UserID.DataPropertyName = "UserID";
            this.UserID.HeaderText = "用户ID";
            this.UserID.MinimumWidth = 8;
            this.UserID.Name = "UserID";
            this.UserID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // Password
            // 
            this.Password.DataPropertyName = "Password";
            this.Password.HeaderText = "用户密码";
            this.Password.MinimumWidth = 8;
            this.Password.Name = "Password";
            this.Password.Visible = false;
            // 
            // UserName
            // 
            this.UserName.DataPropertyName = "UserName";
            this.UserName.HeaderText = "姓名";
            this.UserName.MinimumWidth = 8;
            this.UserName.Name = "UserName";
            // 
            // Gender
            // 
            this.Gender.DataPropertyName = "Gender";
            this.Gender.HeaderText = "性别";
            this.Gender.Items.AddRange(new object[] {
            "男",
            "女"});
            this.Gender.MinimumWidth = 8;
            this.Gender.Name = "Gender";
            this.Gender.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Gender.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // BirthDate
            // 
            this.BirthDate.DataPropertyName = "BirthDate";
            this.BirthDate.HeaderText = "出生日期";
            this.BirthDate.MinimumWidth = 8;
            this.BirthDate.Name = "BirthDate";
            // 
            // PhoneNumber
            // 
            this.PhoneNumber.DataPropertyName = "PhoneNumber";
            this.PhoneNumber.HeaderText = "联系电话";
            this.PhoneNumber.MinimumWidth = 8;
            this.PhoneNumber.Name = "PhoneNumber";
            // 
            // lblCurrentuser
            // 
            this.lblCurrentuser.AutoSize = true;
            this.lblCurrentuser.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrentuser.Location = new System.Drawing.Point(117, 58);
            this.lblCurrentuser.Name = "lblCurrentuser";
            this.lblCurrentuser.Size = new System.Drawing.Size(117, 28);
            this.lblCurrentuser.TabIndex = 2;
            this.lblCurrentuser.Text = "当前用户：";
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblUsername.Location = new System.Drawing.Point(230, 58);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(75, 28);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "用户名";
            // 
            // btnAdduser
            // 
            this.btnAdduser.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAdduser.Location = new System.Drawing.Point(1431, 172);
            this.btnAdduser.Name = "btnAdduser";
            this.btnAdduser.Size = new System.Drawing.Size(151, 65);
            this.btnAdduser.TabIndex = 1;
            this.btnAdduser.Text = "增加用户";
            this.btnAdduser.UseVisualStyleBackColor = true;
            this.btnAdduser.Click += new System.EventHandler(this.btnAdduser_Click);
            // 
            // btnDeleteuser
            // 
            this.btnDeleteuser.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteuser.Location = new System.Drawing.Point(1431, 282);
            this.btnDeleteuser.Name = "btnDeleteuser";
            this.btnDeleteuser.Size = new System.Drawing.Size(151, 65);
            this.btnDeleteuser.TabIndex = 1;
            this.btnDeleteuser.Text = "删除用户";
            this.btnDeleteuser.UseVisualStyleBackColor = true;
            this.btnDeleteuser.Click += new System.EventHandler(this.btnDeleteuser_Click);
            // 
            // btnModifyconfirm
            // 
            this.btnModifyconfirm.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnModifyconfirm.Location = new System.Drawing.Point(1431, 391);
            this.btnModifyconfirm.Name = "btnModifyconfirm";
            this.btnModifyconfirm.Size = new System.Drawing.Size(151, 66);
            this.btnModifyconfirm.TabIndex = 1;
            this.btnModifyconfirm.Text = "保存修改";
            this.btnModifyconfirm.UseVisualStyleBackColor = true;
            this.btnModifyconfirm.Click += new System.EventHandler(this.btnModifyconfirm_Click);
            // 
            // btnBackToMain
            // 
            this.btnBackToMain.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBackToMain.Location = new System.Drawing.Point(1410, 814);
            this.btnBackToMain.Name = "btnBackToMain";
            this.btnBackToMain.Size = new System.Drawing.Size(172, 61);
            this.btnBackToMain.TabIndex = 3;
            this.btnBackToMain.Text = "退出到主菜单";
            this.btnBackToMain.UseVisualStyleBackColor = true;
            this.btnBackToMain.Click += new System.EventHandler(this.btnBackToMain_Click);
            // 
            // userBindingSource
            // 
            this.userBindingSource.DataSource = typeof(AdminApp.UserManagementForm.User);
            // 
            // UserManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1624, 996);
            this.ControlBox = false;
            this.Controls.Add(this.btnBackToMain);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblCurrentuser);
            this.Controls.Add(this.btnModifyconfirm);
            this.Controls.Add(this.btnDeleteuser);
            this.Controls.Add(this.btnAdduser);
            this.Controls.Add(this.dgvUsers);
            this.Name = "UserManagementForm";
            this.ShowInTaskbar = false;
            this.Text = "用户维护";
            this.Load += new System.EventHandler(this.UserManagementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.userBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataGridView dgvUsers;
        private Label lblCurrentuser;
        private Label lblUsername;
        private Button btnAdduser;
        private Button btnDeleteuser;
        private Button btnModifyconfirm;
        private Button btnBackToMain;
        private BindingSource userBindingSource;
        private DataGridViewTextBoxColumn UserID;
        private DataGridViewTextBoxColumn Password;
        private DataGridViewTextBoxColumn UserName;
        private DataGridViewComboBoxColumn Gender;
        private DataGridViewTextBoxColumn BirthDate;
        private DataGridViewTextBoxColumn PhoneNumber;
    }
}