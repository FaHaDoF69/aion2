namespace Aion2Helper.Forms
{
    partial class MonitoredItemEditForm
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
            this.lblItemName = new Label();
            this.txtItemName = new TextBox();
            this.lblCategory = new Label();
            this.txtCategory = new TextBox();
            this.lblTargetMinPrice = new Label();
            this.numTargetMinPrice = new NumericUpDown();
            this.lblTargetMaxPrice = new Label();
            this.numTargetMaxPrice = new NumericUpDown();
            this.lblExpectedProfitRate = new Label();
            this.numExpectedProfitRate = new NumericUpDown();
            this.lblMaxQuantity = new Label();
            this.numMaxQuantity = new NumericUpDown();
            this.lblPriority = new Label();
            this.numPriority = new NumericUpDown();
            this.chkIsEnabled = new CheckBox();
            this.chkAutoPurchaseEnabled = new CheckBox();
            this.lblMonitorStrategy = new Label();
            this.txtMonitorStrategy = new TextBox();
            this.lblRiskLevel = new Label();
            this.cmbRiskLevel = new ComboBox();
            this.lblNotes = new Label();
            this.txtNotes = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.groupBoxBasic = new GroupBox();
            this.groupBoxPricing = new GroupBox();
            this.groupBoxSettings = new GroupBox();
            this.groupBoxStats = new GroupBox();
            this.lblTotalFoundCount = new Label();
            this.lblTotalPurchaseCount = new Label();
            this.lblLastFoundAt = new Label();
            this.lblLastFoundPrice = new Label();
            
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMinPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMaxPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numExpectedProfitRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxQuantity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPriority)).BeginInit();
            this.groupBoxBasic.SuspendLayout();
            this.groupBoxPricing.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.groupBoxStats.SuspendLayout();
            this.SuspendLayout();

            // 
            // 基本信息组
            // 
            this.groupBoxBasic.Controls.Add(this.lblItemName);
            this.groupBoxBasic.Controls.Add(this.txtItemName);
            this.groupBoxBasic.Controls.Add(this.lblCategory);
            this.groupBoxBasic.Controls.Add(this.txtCategory);
            this.groupBoxBasic.Location = new Point(12, 12);
            this.groupBoxBasic.Name = "groupBoxBasic";
            this.groupBoxBasic.Size = new Size(360, 100);
            this.groupBoxBasic.TabIndex = 0;
            this.groupBoxBasic.TabStop = false;
            this.groupBoxBasic.Text = "基本信息";

            // 
            // lblItemName
            // 
            this.lblItemName.AutoSize = true;
            this.lblItemName.Location = new Point(15, 25);
            this.lblItemName.Name = "lblItemName";
            this.lblItemName.Size = new Size(68, 17);
            this.lblItemName.TabIndex = 0;
            this.lblItemName.Text = "物品名称:";

            // 
            // txtItemName
            // 
            this.txtItemName.Location = new Point(90, 22);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Size = new Size(250, 23);
            this.txtItemName.TabIndex = 1;

            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new Point(15, 55);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new Size(68, 17);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Text = "物品类别:";

            // 
            // txtCategory
            // 
            this.txtCategory.Location = new Point(90, 52);
            this.txtCategory.Name = "txtCategory";
            this.txtCategory.Size = new Size(250, 23);
            this.txtCategory.TabIndex = 3;

            // 
            // 价格设置组
            // 
            this.groupBoxPricing.Controls.Add(this.lblTargetMinPrice);
            this.groupBoxPricing.Controls.Add(this.numTargetMinPrice);
            this.groupBoxPricing.Controls.Add(this.lblTargetMaxPrice);
            this.groupBoxPricing.Controls.Add(this.numTargetMaxPrice);
            this.groupBoxPricing.Controls.Add(this.lblExpectedProfitRate);
            this.groupBoxPricing.Controls.Add(this.numExpectedProfitRate);
            this.groupBoxPricing.Location = new Point(12, 125);
            this.groupBoxPricing.Name = "groupBoxPricing";
            this.groupBoxPricing.Size = new Size(360, 120);
            this.groupBoxPricing.TabIndex = 1;
            this.groupBoxPricing.TabStop = false;
            this.groupBoxPricing.Text = "价格设置";

            // 
            // lblTargetMinPrice
            // 
            this.lblTargetMinPrice.AutoSize = true;
            this.lblTargetMinPrice.Location = new Point(15, 25);
            this.lblTargetMinPrice.Name = "lblTargetMinPrice";
            this.lblTargetMinPrice.Size = new Size(68, 17);
            this.lblTargetMinPrice.TabIndex = 0;
            this.lblTargetMinPrice.Text = "最低价格:";

            // 
            // numTargetMinPrice
            // 
            this.numTargetMinPrice.Location = new Point(90, 22);
            this.numTargetMinPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            this.numTargetMinPrice.Name = "numTargetMinPrice";
            this.numTargetMinPrice.Size = new Size(120, 23);
            this.numTargetMinPrice.TabIndex = 1;
            this.numTargetMinPrice.ThousandsSeparator = true;

            // 
            // lblTargetMaxPrice
            // 
            this.lblTargetMaxPrice.AutoSize = true;
            this.lblTargetMaxPrice.Location = new Point(15, 55);
            this.lblTargetMaxPrice.Name = "lblTargetMaxPrice";
            this.lblTargetMaxPrice.Size = new Size(68, 17);
            this.lblTargetMaxPrice.TabIndex = 2;
            this.lblTargetMaxPrice.Text = "最高价格:";

            // 
            // numTargetMaxPrice
            // 
            this.numTargetMaxPrice.Location = new Point(90, 52);
            this.numTargetMaxPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            this.numTargetMaxPrice.Name = "numTargetMaxPrice";
            this.numTargetMaxPrice.Size = new Size(120, 23);
            this.numTargetMaxPrice.TabIndex = 3;
            this.numTargetMaxPrice.ThousandsSeparator = true;

            // 
            // lblExpectedProfitRate
            // 
            this.lblExpectedProfitRate.AutoSize = true;
            this.lblExpectedProfitRate.Location = new Point(15, 85);
            this.lblExpectedProfitRate.Name = "lblExpectedProfitRate";
            this.lblExpectedProfitRate.Size = new Size(68, 17);
            this.lblExpectedProfitRate.TabIndex = 4;
            this.lblExpectedProfitRate.Text = "期望利润率(%):";

            // 
            // numExpectedProfitRate
            // 
            this.numExpectedProfitRate.DecimalPlaces = 1;
            this.numExpectedProfitRate.Location = new Point(120, 82);
            this.numExpectedProfitRate.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numExpectedProfitRate.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numExpectedProfitRate.Name = "numExpectedProfitRate";
            this.numExpectedProfitRate.Size = new Size(90, 23);
            this.numExpectedProfitRate.TabIndex = 5;
            this.numExpectedProfitRate.Value = new decimal(new int[] { 20, 0, 0, 0 });

            // 
            // 监控设置组
            // 
            this.groupBoxSettings.Controls.Add(this.lblMaxQuantity);
            this.groupBoxSettings.Controls.Add(this.numMaxQuantity);
            this.groupBoxSettings.Controls.Add(this.lblPriority);
            this.groupBoxSettings.Controls.Add(this.numPriority);
            this.groupBoxSettings.Controls.Add(this.chkIsEnabled);
            this.groupBoxSettings.Controls.Add(this.chkAutoPurchaseEnabled);
            this.groupBoxSettings.Controls.Add(this.lblMonitorStrategy);
            this.groupBoxSettings.Controls.Add(this.txtMonitorStrategy);
            this.groupBoxSettings.Controls.Add(this.lblRiskLevel);
            this.groupBoxSettings.Controls.Add(this.cmbRiskLevel);
            this.groupBoxSettings.Location = new Point(12, 255);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new Size(360, 180);
            this.groupBoxSettings.TabIndex = 2;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "监控设置";

            // 
            // lblMaxQuantity
            // 
            this.lblMaxQuantity.AutoSize = true;
            this.lblMaxQuantity.Location = new Point(15, 25);
            this.lblMaxQuantity.Name = "lblMaxQuantity";
            this.lblMaxQuantity.Size = new Size(68, 17);
            this.lblMaxQuantity.TabIndex = 0;
            this.lblMaxQuantity.Text = "最大数量:";

            // 
            // numMaxQuantity
            // 
            this.numMaxQuantity.Location = new Point(90, 22);
            this.numMaxQuantity.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            this.numMaxQuantity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxQuantity.Name = "numMaxQuantity";
            this.numMaxQuantity.Size = new Size(80, 23);
            this.numMaxQuantity.TabIndex = 1;
            this.numMaxQuantity.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // lblPriority
            // 
            this.lblPriority.AutoSize = true;
            this.lblPriority.Location = new Point(200, 25);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new Size(44, 17);
            this.lblPriority.TabIndex = 2;
            this.lblPriority.Text = "优先级:";

            // 
            // numPriority
            // 
            this.numPriority.Location = new Point(250, 22);
            this.numPriority.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numPriority.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPriority.Name = "numPriority";
            this.numPriority.Size = new Size(80, 23);
            this.numPriority.TabIndex = 3;
            this.numPriority.Value = new decimal(new int[] { 5, 0, 0, 0 });

            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.AutoSize = true;
            this.chkIsEnabled.Location = new Point(15, 55);
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.Size = new Size(75, 21);
            this.chkIsEnabled.TabIndex = 4;
            this.chkIsEnabled.Text = "启用监控";
            this.chkIsEnabled.UseVisualStyleBackColor = true;

            // 
            // chkAutoPurchaseEnabled
            // 
            this.chkAutoPurchaseEnabled.AutoSize = true;
            this.chkAutoPurchaseEnabled.Location = new Point(120, 55);
            this.chkAutoPurchaseEnabled.Name = "chkAutoPurchaseEnabled";
            this.chkAutoPurchaseEnabled.Size = new Size(75, 21);
            this.chkAutoPurchaseEnabled.TabIndex = 5;
            this.chkAutoPurchaseEnabled.Text = "自动购买";
            this.chkAutoPurchaseEnabled.UseVisualStyleBackColor = true;

            // 
            // lblMonitorStrategy
            // 
            this.lblMonitorStrategy.AutoSize = true;
            this.lblMonitorStrategy.Location = new Point(15, 85);
            this.lblMonitorStrategy.Name = "lblMonitorStrategy";
            this.lblMonitorStrategy.Size = new Size(68, 17);
            this.lblMonitorStrategy.TabIndex = 6;
            this.lblMonitorStrategy.Text = "监控策略:";

            // 
            // txtMonitorStrategy
            // 
            this.txtMonitorStrategy.Location = new Point(90, 82);
            this.txtMonitorStrategy.Name = "txtMonitorStrategy";
            this.txtMonitorStrategy.Size = new Size(120, 23);
            this.txtMonitorStrategy.TabIndex = 7;

            // 
            // lblRiskLevel
            // 
            this.lblRiskLevel.AutoSize = true;
            this.lblRiskLevel.Location = new Point(15, 115);
            this.lblRiskLevel.Name = "lblRiskLevel";
            this.lblRiskLevel.Size = new Size(68, 17);
            this.lblRiskLevel.TabIndex = 8;
            this.lblRiskLevel.Text = "风险等级:";

            // 
            // cmbRiskLevel
            // 
            this.cmbRiskLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbRiskLevel.FormattingEnabled = true;
            this.cmbRiskLevel.Items.AddRange(new object[] { "低风险", "中等风险", "高风险", "极高风险" });
            this.cmbRiskLevel.Location = new Point(90, 112);
            this.cmbRiskLevel.Name = "cmbRiskLevel";
            this.cmbRiskLevel.Size = new Size(120, 25);
            this.cmbRiskLevel.TabIndex = 9;

            // 
            // lblNotes
            // 
            this.lblNotes.AutoSize = true;
            this.lblNotes.Location = new Point(15, 145);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new Size(44, 17);
            this.lblNotes.TabIndex = 10;
            this.lblNotes.Text = "备注:";

            // 
            // txtNotes
            // 
            this.txtNotes.Location = new Point(90, 142);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new Size(250, 23);
            this.txtNotes.TabIndex = 11;

            this.groupBoxSettings.Controls.Add(this.lblNotes);
            this.groupBoxSettings.Controls.Add(this.txtNotes);

            // 
            // 统计信息组
            // 
            this.groupBoxStats.Controls.Add(this.lblTotalFoundCount);
            this.groupBoxStats.Controls.Add(this.lblTotalPurchaseCount);
            this.groupBoxStats.Controls.Add(this.lblLastFoundAt);
            this.groupBoxStats.Controls.Add(this.lblLastFoundPrice);
            this.groupBoxStats.Location = new Point(12, 445);
            this.groupBoxStats.Name = "groupBoxStats";
            this.groupBoxStats.Size = new Size(360, 100);
            this.groupBoxStats.TabIndex = 3;
            this.groupBoxStats.TabStop = false;
            this.groupBoxStats.Text = "统计信息";

            // 
            // lblTotalFoundCount
            // 
            this.lblTotalFoundCount.AutoSize = true;
            this.lblTotalFoundCount.Location = new Point(15, 25);
            this.lblTotalFoundCount.Name = "lblTotalFoundCount";
            this.lblTotalFoundCount.Size = new Size(80, 17);
            this.lblTotalFoundCount.TabIndex = 0;
            this.lblTotalFoundCount.Text = "发现次数: 0";

            // 
            // lblTotalPurchaseCount
            // 
            this.lblTotalPurchaseCount.AutoSize = true;
            this.lblTotalPurchaseCount.Location = new Point(200, 25);
            this.lblTotalPurchaseCount.Name = "lblTotalPurchaseCount";
            this.lblTotalPurchaseCount.Size = new Size(80, 17);
            this.lblTotalPurchaseCount.TabIndex = 1;
            this.lblTotalPurchaseCount.Text = "购买次数: 0";

            // 
            // lblLastFoundAt
            // 
            this.lblLastFoundAt.AutoSize = true;
            this.lblLastFoundAt.Location = new Point(15, 50);
            this.lblLastFoundAt.Name = "lblLastFoundAt";
            this.lblLastFoundAt.Size = new Size(80, 17);
            this.lblLastFoundAt.TabIndex = 2;
            this.lblLastFoundAt.Text = "最后发现: 从未";

            // 
            // lblLastFoundPrice
            // 
            this.lblLastFoundPrice.AutoSize = true;
            this.lblLastFoundPrice.Location = new Point(15, 75);
            this.lblLastFoundPrice.Name = "lblLastFoundPrice";
            this.lblLastFoundPrice.Size = new Size(80, 17);
            this.lblLastFoundPrice.TabIndex = 3;
            this.lblLastFoundPrice.Text = "发现价格: 无";

            // 
            // btnSave
            // 
            this.btnSave.BackColor = Color.FromArgb(40, 167, 69);
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.ForeColor = Color.White;
            this.btnSave.Location = new Point(200, 560);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(80, 35);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(290, 560);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(80, 35);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // 
            // MonitoredItemEditForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 17F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 610);
            this.Controls.Add(this.groupBoxBasic);
            this.Controls.Add(this.groupBoxPricing);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.groupBoxStats);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Name = "MonitoredItemEditForm";
            this.Text = "编辑监控物品";
            
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMinPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMaxPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numExpectedProfitRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxQuantity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPriority)).EndInit();
            this.groupBoxBasic.ResumeLayout(false);
            this.groupBoxBasic.PerformLayout();
            this.groupBoxPricing.ResumeLayout(false);
            this.groupBoxPricing.PerformLayout();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.groupBoxStats.ResumeLayout(false);
            this.groupBoxStats.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Label lblItemName;
        private TextBox txtItemName;
        private Label lblCategory;
        private TextBox txtCategory;
        private Label lblTargetMinPrice;
        private NumericUpDown numTargetMinPrice;
        private Label lblTargetMaxPrice;
        private NumericUpDown numTargetMaxPrice;
        private Label lblExpectedProfitRate;
        private NumericUpDown numExpectedProfitRate;
        private Label lblMaxQuantity;
        private NumericUpDown numMaxQuantity;
        private Label lblPriority;
        private NumericUpDown numPriority;
        private CheckBox chkIsEnabled;
        private CheckBox chkAutoPurchaseEnabled;
        private Label lblMonitorStrategy;
        private TextBox txtMonitorStrategy;
        private Label lblRiskLevel;
        private ComboBox cmbRiskLevel;
        private Label lblNotes;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private GroupBox groupBoxBasic;
        private GroupBox groupBoxPricing;
        private GroupBox groupBoxSettings;
        private GroupBox groupBoxStats;
        private Label lblTotalFoundCount;
        private Label lblTotalPurchaseCount;
        private Label lblLastFoundAt;
        private Label lblLastFoundPrice;
    }
}
