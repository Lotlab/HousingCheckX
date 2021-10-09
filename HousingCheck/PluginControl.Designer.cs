
namespace HousingCheck
{
    partial class PluginControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxTable = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.housingCheckBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pluginControlBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxLog = new System.Windows.Forms.GroupBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.groupBoxControl = new System.Windows.Forms.GroupBox();
            this.buttonCopyToClipboard = new System.Windows.Forms.Button();
            this.buttonSaveToFile = new System.Windows.Forms.Button();
            this.buttonUploadOnce = new System.Windows.Forms.Button();
            this.groupBoxUpload = new System.Windows.Forms.GroupBox();
            this.selectApiVersion = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxUploadSnapshot = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUploadToken = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxUpload = new System.Windows.Forms.TextBox();
            this.checkBoxUpload = new System.Windows.Forms.CheckBox();
            this.groupBoxNotify = new System.Windows.Forms.GroupBox();
            this.groupBoxNotifyCheck = new System.Windows.Forms.GroupBox();
            this.buttonNotifyCheckTest = new System.Windows.Forms.Button();
            this.numericUpDownNotifyCheck = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxNotifyCheck = new System.Windows.Forms.CheckBox();
            this.groupBoxNotifyHouse = new System.Windows.Forms.GroupBox();
            this.checkBoxNotifyML = new System.Windows.Forms.CheckBox();
            this.checkBoxNotifyS = new System.Windows.Forms.CheckBox();
            this.buttonNotifyTest = new System.Windows.Forms.Button();
            this.checkBoxNotification = new System.Windows.Forms.CheckBox();
            this.checkboxTTS = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBoxTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.housingCheckBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pluginControlBindingSource)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBoxLog.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.groupBoxUpload.SuspendLayout();
            this.groupBoxNotify.SuspendLayout();
            this.groupBoxNotifyCheck.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNotifyCheck)).BeginInit();
            this.groupBoxNotifyHouse.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTable
            // 
            this.groupBoxTable.Controls.Add(this.dataGridView1);
            this.groupBoxTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTable.Location = new System.Drawing.Point(14, 6);
            this.groupBoxTable.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBoxTable.Name = "groupBoxTable";
            this.groupBoxTable.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBoxTable.Size = new System.Drawing.Size(568, 642);
            this.groupBoxTable.TabIndex = 0;
            this.groupBoxTable.TabStop = false;
            this.groupBoxTable.Text = "在售列表";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(7, 20);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 50;
            this.dataGridView1.Size = new System.Drawing.Size(554, 616);
            this.dataGridView1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxLog);
            this.panel1.Controls.Add(this.groupBoxControl);
            this.panel1.Controls.Add(this.groupBoxUpload);
            this.panel1.Controls.Add(this.groupBoxNotify);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(589, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(7, 6, 14, 7);
            this.panel1.Size = new System.Drawing.Size(351, 655);
            this.panel1.TabIndex = 4;
            // 
            // groupBoxLog
            // 
            this.groupBoxLog.Controls.Add(this.textBoxLog);
            this.groupBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLog.Location = new System.Drawing.Point(7, 315);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBoxLog.Size = new System.Drawing.Size(330, 333);
            this.groupBoxLog.TabIndex = 12;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "日志";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(7, 20);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(316, 307);
            this.textBoxLog.TabIndex = 0;
            // 
            // groupBoxControl
            // 
            this.groupBoxControl.Controls.Add(this.buttonCopyToClipboard);
            this.groupBoxControl.Controls.Add(this.buttonSaveToFile);
            this.groupBoxControl.Controls.Add(this.buttonUploadOnce);
            this.groupBoxControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxControl.Location = new System.Drawing.Point(7, 266);
            this.groupBoxControl.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Size = new System.Drawing.Size(330, 49);
            this.groupBoxControl.TabIndex = 11;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "操作";
            // 
            // buttonCopyToClipboard
            // 
            this.buttonCopyToClipboard.Location = new System.Drawing.Point(224, 21);
            this.buttonCopyToClipboard.Name = "buttonCopyToClipboard";
            this.buttonCopyToClipboard.Size = new System.Drawing.Size(98, 23);
            this.buttonCopyToClipboard.TabIndex = 0;
            this.buttonCopyToClipboard.Text = "复制到剪贴板";
            this.buttonCopyToClipboard.UseVisualStyleBackColor = true;
            // 
            // buttonSaveToFile
            // 
            this.buttonSaveToFile.Location = new System.Drawing.Point(117, 21);
            this.buttonSaveToFile.Name = "buttonSaveToFile";
            this.buttonSaveToFile.Size = new System.Drawing.Size(98, 23);
            this.buttonSaveToFile.TabIndex = 0;
            this.buttonSaveToFile.Text = "保存到文件";
            this.buttonSaveToFile.UseVisualStyleBackColor = true;
            // 
            // buttonUploadOnce
            // 
            this.buttonUploadOnce.Location = new System.Drawing.Point(10, 21);
            this.buttonUploadOnce.Name = "buttonUploadOnce";
            this.buttonUploadOnce.Size = new System.Drawing.Size(98, 23);
            this.buttonUploadOnce.TabIndex = 0;
            this.buttonUploadOnce.Text = "手动上报一次";
            this.buttonUploadOnce.UseVisualStyleBackColor = true;
            // 
            // groupBoxUpload
            // 
            this.groupBoxUpload.Controls.Add(this.selectApiVersion);
            this.groupBoxUpload.Controls.Add(this.label4);
            this.groupBoxUpload.Controls.Add(this.checkBoxUploadSnapshot);
            this.groupBoxUpload.Controls.Add(this.label3);
            this.groupBoxUpload.Controls.Add(this.textBoxUploadToken);
            this.groupBoxUpload.Controls.Add(this.label2);
            this.groupBoxUpload.Controls.Add(this.label1);
            this.groupBoxUpload.Controls.Add(this.textBoxUpload);
            this.groupBoxUpload.Controls.Add(this.checkBoxUpload);
            this.groupBoxUpload.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxUpload.Location = new System.Drawing.Point(7, 158);
            this.groupBoxUpload.Margin = new System.Windows.Forms.Padding(7, 6, 7, 12);
            this.groupBoxUpload.Name = "groupBoxUpload";
            this.groupBoxUpload.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.groupBoxUpload.Size = new System.Drawing.Size(330, 108);
            this.groupBoxUpload.TabIndex = 10;
            this.groupBoxUpload.TabStop = false;
            this.groupBoxUpload.Text = "上报设置";
            // 
            // selectApiVersion
            // 
            this.selectApiVersion.DataSource = this.pluginControlBindingSource;
            this.selectApiVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectApiVersion.FormattingEnabled = true;
            this.selectApiVersion.Location = new System.Drawing.Point(271, 20);
            this.selectApiVersion.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.selectApiVersion.Name = "selectApiVersion";
            this.selectApiVersion.Size = new System.Drawing.Size(52, 20);
            this.selectApiVersion.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(197, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "上报API版本:";
            // 
            // checkBoxUploadSnapshot
            // 
            this.checkBoxUploadSnapshot.AutoSize = true;
            this.checkBoxUploadSnapshot.Checked = true;
            this.checkBoxUploadSnapshot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUploadSnapshot.Enabled = false;
            this.checkBoxUploadSnapshot.Location = new System.Drawing.Point(105, 22);
            this.checkBoxUploadSnapshot.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxUploadSnapshot.Name = "checkBoxUploadSnapshot";
            this.checkBoxUploadSnapshot.Size = new System.Drawing.Size(96, 16);
            this.checkBoxUploadSnapshot.TabIndex = 7;
            this.checkBoxUploadSnapshot.Text = "上报房区快照";
            this.checkBoxUploadSnapshot.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(150, 91);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "如服务器未开启身份验证请留空";
            // 
            // textBoxUploadToken
            // 
            this.textBoxUploadToken.Enabled = false;
            this.textBoxUploadToken.Location = new System.Drawing.Point(75, 66);
            this.textBoxUploadToken.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxUploadToken.Name = "textBoxUploadToken";
            this.textBoxUploadToken.ReadOnly = true;
            this.textBoxUploadToken.Size = new System.Drawing.Size(248, 21);
            this.textBoxUploadToken.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Token:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "上报地址:";
            // 
            // textBoxUpload
            // 
            this.textBoxUpload.Enabled = false;
            this.textBoxUpload.Location = new System.Drawing.Point(75, 45);
            this.textBoxUpload.Name = "textBoxUpload";
            this.textBoxUpload.ReadOnly = true;
            this.textBoxUpload.Size = new System.Drawing.Size(248, 21);
            this.textBoxUpload.TabIndex = 1;
            // 
            // checkBoxUpload
            // 
            this.checkBoxUpload.AutoSize = true;
            this.checkBoxUpload.Location = new System.Drawing.Point(10, 22);
            this.checkBoxUpload.Name = "checkBoxUpload";
            this.checkBoxUpload.Size = new System.Drawing.Size(96, 16);
            this.checkBoxUpload.TabIndex = 0;
            this.checkBoxUpload.Text = "开启自动上报";
            this.checkBoxUpload.UseVisualStyleBackColor = true;
            this.checkBoxUpload.CheckedChanged += new System.EventHandler(this.checkBoxUpload_CheckedChanged);
            // 
            // groupBoxNotify
            // 
            this.groupBoxNotify.Controls.Add(this.groupBoxNotifyCheck);
            this.groupBoxNotify.Controls.Add(this.groupBoxNotifyHouse);
            this.groupBoxNotify.Controls.Add(this.checkBoxNotification);
            this.groupBoxNotify.Controls.Add(this.checkboxTTS);
            this.groupBoxNotify.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxNotify.Location = new System.Drawing.Point(7, 6);
            this.groupBoxNotify.Name = "groupBoxNotify";
            this.groupBoxNotify.Size = new System.Drawing.Size(330, 152);
            this.groupBoxNotify.TabIndex = 9;
            this.groupBoxNotify.TabStop = false;
            this.groupBoxNotify.Text = "通知设置";
            // 
            // groupBoxNotifyCheck
            // 
            this.groupBoxNotifyCheck.Controls.Add(this.buttonNotifyCheckTest);
            this.groupBoxNotifyCheck.Controls.Add(this.numericUpDownNotifyCheck);
            this.groupBoxNotifyCheck.Controls.Add(this.label5);
            this.groupBoxNotifyCheck.Controls.Add(this.checkBoxNotifyCheck);
            this.groupBoxNotifyCheck.Location = new System.Drawing.Point(11, 95);
            this.groupBoxNotifyCheck.Name = "groupBoxNotifyCheck";
            this.groupBoxNotifyCheck.Size = new System.Drawing.Size(311, 46);
            this.groupBoxNotifyCheck.TabIndex = 4;
            this.groupBoxNotifyCheck.TabStop = false;
            this.groupBoxNotifyCheck.Text = "查房提醒";
            // 
            // buttonNotifyCheckTest
            // 
            this.buttonNotifyCheckTest.Location = new System.Drawing.Point(243, 16);
            this.buttonNotifyCheckTest.Name = "buttonNotifyCheckTest";
            this.buttonNotifyCheckTest.Size = new System.Drawing.Size(62, 23);
            this.buttonNotifyCheckTest.TabIndex = 4;
            this.buttonNotifyCheckTest.Text = "测试";
            this.buttonNotifyCheckTest.UseVisualStyleBackColor = true;
            // 
            // numericUpDownNotifyCheck
            // 
            this.numericUpDownNotifyCheck.Location = new System.Drawing.Point(177, 18);
            this.numericUpDownNotifyCheck.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownNotifyCheck.Name = "numericUpDownNotifyCheck";
            this.numericUpDownNotifyCheck.Size = new System.Drawing.Size(43, 21);
            this.numericUpDownNotifyCheck.TabIndex = 3;
            this.numericUpDownNotifyCheck.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(89, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "提前时间 (s)：";
            // 
            // checkBoxNotifyCheck
            // 
            this.checkBoxNotifyCheck.AutoSize = true;
            this.checkBoxNotifyCheck.Location = new System.Drawing.Point(11, 21);
            this.checkBoxNotifyCheck.Name = "checkBoxNotifyCheck";
            this.checkBoxNotifyCheck.Size = new System.Drawing.Size(72, 16);
            this.checkBoxNotifyCheck.TabIndex = 0;
            this.checkBoxNotifyCheck.Text = "整点提醒";
            this.checkBoxNotifyCheck.UseVisualStyleBackColor = true;
            // 
            // groupBoxNotifyHouse
            // 
            this.groupBoxNotifyHouse.Controls.Add(this.checkBoxNotifyML);
            this.groupBoxNotifyHouse.Controls.Add(this.checkBoxNotifyS);
            this.groupBoxNotifyHouse.Controls.Add(this.buttonNotifyTest);
            this.groupBoxNotifyHouse.Location = new System.Drawing.Point(11, 43);
            this.groupBoxNotifyHouse.Name = "groupBoxNotifyHouse";
            this.groupBoxNotifyHouse.Size = new System.Drawing.Size(311, 46);
            this.groupBoxNotifyHouse.TabIndex = 3;
            this.groupBoxNotifyHouse.TabStop = false;
            this.groupBoxNotifyHouse.Text = "空房通知";
            // 
            // checkBoxNotifyML
            // 
            this.checkBoxNotifyML.AutoSize = true;
            this.checkBoxNotifyML.Checked = true;
            this.checkBoxNotifyML.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNotifyML.Enabled = false;
            this.checkBoxNotifyML.Location = new System.Drawing.Point(64, 20);
            this.checkBoxNotifyML.Name = "checkBoxNotifyML";
            this.checkBoxNotifyML.Size = new System.Drawing.Size(54, 16);
            this.checkBoxNotifyML.TabIndex = 5;
            this.checkBoxNotifyML.Text = "M/L房";
            this.checkBoxNotifyML.UseVisualStyleBackColor = true;
            // 
            // checkBoxNotifyS
            // 
            this.checkBoxNotifyS.AutoSize = true;
            this.checkBoxNotifyS.Location = new System.Drawing.Point(11, 20);
            this.checkBoxNotifyS.Name = "checkBoxNotifyS";
            this.checkBoxNotifyS.Size = new System.Drawing.Size(42, 16);
            this.checkBoxNotifyS.TabIndex = 4;
            this.checkBoxNotifyS.Text = "S房";
            this.checkBoxNotifyS.UseVisualStyleBackColor = true;
            // 
            // buttonNotifyTest
            // 
            this.buttonNotifyTest.Location = new System.Drawing.Point(243, 16);
            this.buttonNotifyTest.Name = "buttonNotifyTest";
            this.buttonNotifyTest.Size = new System.Drawing.Size(62, 23);
            this.buttonNotifyTest.TabIndex = 3;
            this.buttonNotifyTest.Text = "测试";
            this.buttonNotifyTest.UseVisualStyleBackColor = true;
            // 
            // checkBoxNotification
            // 
            this.checkBoxNotification.AutoSize = true;
            this.checkBoxNotification.Location = new System.Drawing.Point(88, 20);
            this.checkBoxNotification.Name = "checkBoxNotification";
            this.checkBoxNotification.Size = new System.Drawing.Size(120, 16);
            this.checkBoxNotification.TabIndex = 1;
            this.checkBoxNotification.Text = "弹出通知 (Win8+)";
            this.checkBoxNotification.UseVisualStyleBackColor = true;
            // 
            // checkboxTTS
            // 
            this.checkboxTTS.AutoSize = true;
            this.checkboxTTS.Location = new System.Drawing.Point(10, 20);
            this.checkboxTTS.Name = "checkboxTTS";
            this.checkboxTTS.Size = new System.Drawing.Size(72, 16);
            this.checkboxTTS.TabIndex = 0;
            this.checkboxTTS.Text = "语音播报";
            this.checkboxTTS.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBoxTable);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(14, 6, 7, 7);
            this.panel2.Size = new System.Drawing.Size(589, 655);
            this.panel2.TabIndex = 5;
            // 
            // PluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "PluginControl";
            this.Size = new System.Drawing.Size(940, 655);
            this.groupBoxTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.housingCheckBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pluginControlBindingSource)).EndInit();
            this.panel1.ResumeLayout(false);
            this.groupBoxLog.ResumeLayout(false);
            this.groupBoxLog.PerformLayout();
            this.groupBoxControl.ResumeLayout(false);
            this.groupBoxUpload.ResumeLayout(false);
            this.groupBoxUpload.PerformLayout();
            this.groupBoxNotify.ResumeLayout(false);
            this.groupBoxNotify.PerformLayout();
            this.groupBoxNotifyCheck.ResumeLayout(false);
            this.groupBoxNotifyCheck.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNotifyCheck)).EndInit();
            this.groupBoxNotifyHouse.ResumeLayout(false);
            this.groupBoxNotifyHouse.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTable;
        private System.Windows.Forms.BindingSource housingCheckBindingSource;
        public System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.BindingSource pluginControlBindingSource;
        private System.Windows.Forms.GroupBox groupBoxLog;
        public System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.GroupBox groupBoxControl;
        public System.Windows.Forms.Button buttonCopyToClipboard;
        public System.Windows.Forms.Button buttonSaveToFile;
        public System.Windows.Forms.Button buttonUploadOnce;
        private System.Windows.Forms.GroupBox groupBoxUpload;
        public System.Windows.Forms.ComboBox selectApiVersion;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.CheckBox checkBoxUploadSnapshot;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox textBoxUploadToken;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBoxUpload;
        private System.Windows.Forms.CheckBox checkBoxUpload;
        private System.Windows.Forms.GroupBox groupBoxNotify;
        private System.Windows.Forms.CheckBox checkBoxNotification;
        private System.Windows.Forms.CheckBox checkboxTTS;
        private System.Windows.Forms.GroupBox groupBoxNotifyCheck;
        public System.Windows.Forms.Button buttonNotifyCheckTest;
        private System.Windows.Forms.NumericUpDown numericUpDownNotifyCheck;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxNotifyCheck;
        private System.Windows.Forms.GroupBox groupBoxNotifyHouse;
        private System.Windows.Forms.CheckBox checkBoxNotifyML;
        private System.Windows.Forms.CheckBox checkBoxNotifyS;
        public System.Windows.Forms.Button buttonNotifyTest;
    }
}
