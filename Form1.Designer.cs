namespace Aion2Helper
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new TabControl();
            this.tabPageMonitor = new TabPage();
            this.tabPageSettings = new TabPage();
            this.tabPageTrading = new TabPage();
            this.tabPageHistory = new TabPage();
            this.groupBoxControl = new GroupBox();
            this.btnStart = new Button();
            this.btnStop = new Button();
            this.btnSettings = new Button();
            this.groupBoxStats = new GroupBox();
            this.lblMonitoredItems = new Label();
            this.lblOpportunities = new Label();
            this.lblExecutedTrades = new Label();
            this.lblTotalProfit = new Label();
            this.listViewOpportunities = new ListView();
            this.textBoxLog = new TextBox();
            this.statusStrip1 = new StatusStrip();
            this.toolStripStatusLabel1 = new ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new ToolStripStatusLabel();
            
            // 设置窗体
            this.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageMonitor.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.groupBoxStats.SuspendLayout();
            this.statusStrip1.SuspendLayout();

            // tabControl1
            this.tabControl1.Controls.Add(this.tabPageMonitor);
            this.tabControl1.Controls.Add(this.tabPageSettings);
            this.tabControl1.Controls.Add(this.tabPageTrading);
            this.tabControl1.Controls.Add(this.tabPageHistory);
            this.tabControl1.Dock = DockStyle.Fill;
            this.tabControl1.Location = new Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(1440, 840);
            this.tabControl1.TabIndex = 0;

            // tabPageMonitor
            this.tabPageMonitor.Controls.Add(this.groupBoxControl);
            this.tabPageMonitor.Controls.Add(this.groupBoxStats);
            this.tabPageMonitor.Controls.Add(this.listViewOpportunities);
            this.tabPageMonitor.Controls.Add(this.textBoxLog);
            this.tabPageMonitor.Location = new Point(4, 24);
            this.tabPageMonitor.Name = "tabPageMonitor";
            this.tabPageMonitor.Padding = new Padding(3);
            this.tabPageMonitor.Size = new Size(992, 572);
            this.tabPageMonitor.TabIndex = 0;
            this.tabPageMonitor.Text = "📊 实时监控";
            this.tabPageMonitor.UseVisualStyleBackColor = true;

            // groupBoxControl
            this.groupBoxControl.Controls.Add(this.btnStart);
            this.groupBoxControl.Controls.Add(this.btnStop);
            this.groupBoxControl.Controls.Add(this.btnSettings);
            this.groupBoxControl.Location = new Point(8, 6);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Size = new Size(1411, 72);
            this.groupBoxControl.TabIndex = 0;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "控制面板";

            // btnStart
            this.btnStart.BackColor = Color.FromArgb(40, 167, 69);
            this.btnStart.FlatStyle = FlatStyle.Flat;
            this.btnStart.ForeColor = Color.White;
            this.btnStart.Location = new Point(15, 22);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new Size(100, 30);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "▶ 开始监控";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new EventHandler(this.btnStart_Click);

            // btnStop
            this.btnStop.BackColor = Color.FromArgb(220, 53, 69);
            this.btnStop.Enabled = false;
            this.btnStop.FlatStyle = FlatStyle.Flat;
            this.btnStop.ForeColor = Color.White;
            this.btnStop.Location = new Point(125, 22);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new Size(100, 30);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "⏸ 停止监控";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new EventHandler(this.btnStop_Click);

            // btnSettings
            this.btnSettings.BackColor = Color.FromArgb(0, 123, 255);
            this.btnSettings.FlatStyle = FlatStyle.Flat;
            this.btnSettings.ForeColor = Color.White;
            this.btnSettings.Location = new Point(235, 22);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new Size(80, 30);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.Text = "⚙ 监控设置";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new EventHandler(this.btnSettings_Click);

            // 测试检测按钮
            this.btnTestDetection = new Button();
            this.btnTestDetection.BackColor = Color.FromArgb(255, 193, 7);
            this.btnTestDetection.FlatStyle = FlatStyle.Flat;
            this.btnTestDetection.ForeColor = Color.Black;
            this.btnTestDetection.Location = new Point(325, 22);
            this.btnTestDetection.Name = "btnTestDetection";
            this.btnTestDetection.Size = new Size(100, 30);
            this.btnTestDetection.TabIndex = 3;
            this.btnTestDetection.Text = "🎯 测试检测";
            this.btnTestDetection.UseVisualStyleBackColor = false;
            this.btnTestDetection.Click += new EventHandler(this.btnTestDetection_Click);
            this.groupBoxControl.Controls.Add(this.btnTestDetection);

            // groupBoxStats
            this.groupBoxStats.Controls.Add(this.lblMonitoredItems);
            this.groupBoxStats.Controls.Add(this.lblOpportunities);
            this.groupBoxStats.Controls.Add(this.lblExecutedTrades);
            this.groupBoxStats.Controls.Add(this.lblTotalProfit);
            this.groupBoxStats.Location = new Point(8, 90);
            this.groupBoxStats.Name = "groupBoxStats";
            this.groupBoxStats.Size = new Size(1411, 96);
            this.groupBoxStats.TabIndex = 1;
            this.groupBoxStats.TabStop = false;
            this.groupBoxStats.Text = "监控统计";

            // 统计标签
            this.lblMonitoredItems.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            this.lblMonitoredItems.ForeColor = Color.FromArgb(0, 123, 255);
            this.lblMonitoredItems.Location = new Point(20, 25);
            this.lblMonitoredItems.Name = "lblMonitoredItems";
            this.lblMonitoredItems.Size = new Size(200, 40);
            this.lblMonitoredItems.TabIndex = 0;
            this.lblMonitoredItems.Text = "监控物品: 0";
            this.lblMonitoredItems.TextAlign = ContentAlignment.MiddleCenter;

            this.lblOpportunities.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            this.lblOpportunities.ForeColor = Color.FromArgb(40, 167, 69);
            this.lblOpportunities.Location = new Point(240, 25);
            this.lblOpportunities.Name = "lblOpportunities";
            this.lblOpportunities.Size = new Size(200, 40);
            this.lblOpportunities.TabIndex = 1;
            this.lblOpportunities.Text = "发现机会: 0";
            this.lblOpportunities.TextAlign = ContentAlignment.MiddleCenter;

            this.lblExecutedTrades.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            this.lblExecutedTrades.ForeColor = Color.FromArgb(253, 126, 20);
            this.lblExecutedTrades.Location = new Point(460, 25);
            this.lblExecutedTrades.Name = "lblExecutedTrades";
            this.lblExecutedTrades.Size = new Size(200, 40);
            this.lblExecutedTrades.TabIndex = 2;
            this.lblExecutedTrades.Text = "执行交易: 0";
            this.lblExecutedTrades.TextAlign = ContentAlignment.MiddleCenter;

            this.lblTotalProfit.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            this.lblTotalProfit.ForeColor = Color.FromArgb(32, 201, 151);
            this.lblTotalProfit.Location = new Point(680, 25);
            this.lblTotalProfit.Name = "lblTotalProfit";
            this.lblTotalProfit.Size = new Size(200, 40);
            this.lblTotalProfit.TabIndex = 3;
            this.lblTotalProfit.Text = "总盈利: 0";
            this.lblTotalProfit.TextAlign = ContentAlignment.MiddleCenter;

            // listViewOpportunities
            this.listViewOpportunities.FullRowSelect = true;
            this.listViewOpportunities.GridLines = true;
            this.listViewOpportunities.Location = new Point(8, 192);
            this.listViewOpportunities.Name = "listViewOpportunities";
            this.listViewOpportunities.Size = new Size(1411, 360);
            this.listViewOpportunities.TabIndex = 2;
            this.listViewOpportunities.UseCompatibleStateImageBehavior = false;
            this.listViewOpportunities.View = View.Details;
            this.listViewOpportunities.Columns.Add("物品名称", 150);
            this.listViewOpportunities.Columns.Add("等级", 50);
            this.listViewOpportunities.Columns.Add("当前价格", 100);
            this.listViewOpportunities.Columns.Add("预期价格", 100);
            this.listViewOpportunities.Columns.Add("预期利润", 100);
            this.listViewOpportunities.Columns.Add("利润率", 80);
            this.listViewOpportunities.Columns.Add("风险等级", 80);
            this.listViewOpportunities.Columns.Add("策略", 80);
            this.listViewOpportunities.Columns.Add("发现时间", 120);

            // textBoxLog
            this.textBoxLog.BackColor = Color.Black;
            this.textBoxLog.ForeColor = Color.Lime;
            this.textBoxLog.Font = new Font("Consolas", 9F);
            this.textBoxLog.Location = new Point(8, 558);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = ScrollBars.Vertical;
            this.textBoxLog.Size = new Size(1411, 180);
            this.textBoxLog.TabIndex = 3;

            // tabPageSettings - 先创建控件再添加
            this.tabPageSettings.Location = new Point(4, 24);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Padding = new Padding(3);
            this.tabPageSettings.Size = new Size(1432, 812);
            this.tabPageSettings.TabIndex = 1;
            this.tabPageSettings.Text = "⚙ 监控设置";
            this.tabPageSettings.UseVisualStyleBackColor = true;

            // tabPageTrading
            this.tabPageTrading.Location = new Point(4, 24);
            this.tabPageTrading.Name = "tabPageTrading";
            this.tabPageTrading.Padding = new Padding(3);
            this.tabPageTrading.Size = new Size(1432, 812);
            this.tabPageTrading.TabIndex = 2;
            this.tabPageTrading.Text = "💰 交易设置";
            this.tabPageTrading.UseVisualStyleBackColor = true;

            // tabPageHistory
            this.tabPageHistory.Location = new Point(4, 24);
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Size = new Size(1432, 812);
            this.tabPageHistory.TabIndex = 3;
            this.tabPageHistory.Text = "📈 交易历史";
            this.tabPageHistory.UseVisualStyleBackColor = true;

            // statusStrip1
            this.statusStrip1.Items.AddRange(new ToolStripItem[] {
                this.toolStripStatusLabel1,
                this.toolStripStatusLabel2});
            this.statusStrip1.Location = new Point(0, 628);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new Size(1440, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";

            // toolStripStatusLabel1
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new Size(32, 17);
            this.toolStripStatusLabel1.Text = "就绪";

            // toolStripStatusLabel2
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new Size(1393, 17);
            this.toolStripStatusLabel2.Spring = true;
            this.toolStripStatusLabel2.Text = "运行时间: 00:00:00";
            this.toolStripStatusLabel2.TextAlign = ContentAlignment.MiddleRight;

            // Form1
            this.AutoScaleDimensions = new SizeF(7F, 17F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1440, 900);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Aion 2 拍卖行智能辅助系统 v1.0";
            this.Load += new EventHandler(this.Form1_Load);
            
            this.InitializeSettingsControls();
            this.InitializeHistoryControls();
            
            this.tabControl1.ResumeLayout(false);
            this.tabPageMonitor.ResumeLayout(false);
            this.tabPageMonitor.PerformLayout();
            this.tabPageSettings.ResumeLayout(false);
            this.tabPageHistory.ResumeLayout(false);
            this.groupBoxControl.ResumeLayout(false);
            this.groupBoxStats.ResumeLayout(false);
            this.groupBoxItemMonitor.ResumeLayout(false);
            this.groupBoxTradingSettings.ResumeLayout(false);
            this.groupBoxSafetySettings.ResumeLayout(false);
            this.groupBoxHistoryFilter.ResumeLayout(false);
            this.groupBoxHistoryList.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeSettingsControls()
        {
            // 初始化所有控件
            this.groupBoxItemMonitor = new GroupBox();
            this.listViewMonitoredItems = new ListView();
            this.contextMenuMonitoredItems = new ContextMenuStrip();
            this.menuItemEdit = new ToolStripMenuItem();
            this.menuItemDelete = new ToolStripMenuItem();
            this.menuSeparator1 = new ToolStripSeparator();
            this.menuItemToggleStatus = new ToolStripMenuItem();
            this.menuItemToggleAutoBuy = new ToolStripMenuItem();
            this.menuSeparator2 = new ToolStripSeparator();
            this.menuItemRefresh = new ToolStripMenuItem();
            this.btnAddItem = new Button();
            this.btnRemoveItem = new Button();
            this.btnClearItems = new Button();
            this.btnEditItem = new Button();
            this.btnRefreshItems = new Button();
            this.lblItemList = new Label();
            
            
            this.groupBoxTradingSettings = new GroupBox();
            this.lblMaxInvestment = new Label();
            this.textBoxMaxInvestment = new TextBox();
            this.lblPriceThreshold = new Label();
            this.trackBarPriceThreshold = new TrackBar();
            this.lblThresholdValue = new Label();
            this.checkBoxAutoTrading = new CheckBox();
            
            this.groupBoxSafetySettings = new GroupBox();
            this.checkBoxHumanBehavior = new CheckBox();
            this.lblOperationInterval = new Label();
            this.textBoxMinInterval = new TextBox();
            this.textBoxMaxInterval = new TextBox();
            this.lblIntervalUnit = new Label();
            
            this.btnSaveSettings = new Button();

            // 暂停布局
            this.groupBoxItemMonitor.SuspendLayout();
            this.groupBoxTradingSettings.SuspendLayout();
            this.groupBoxSafetySettings.SuspendLayout();

            // 物品监控设置
            this.groupBoxItemMonitor.Controls.Add(this.lblItemList);
            this.groupBoxItemMonitor.Controls.Add(this.listViewMonitoredItems);
            this.groupBoxItemMonitor.Controls.Add(this.btnAddItem);
            this.groupBoxItemMonitor.Controls.Add(this.btnRemoveItem);
            this.groupBoxItemMonitor.Controls.Add(this.btnClearItems);
            this.groupBoxItemMonitor.Controls.Add(this.btnEditItem);
            this.groupBoxItemMonitor.Controls.Add(this.btnRefreshItems);
            
            this.groupBoxItemMonitor.Location = new Point(10, 10);
            this.groupBoxItemMonitor.Name = "groupBoxItemMonitor";
            this.groupBoxItemMonitor.Size = new Size(1170, 525);
            this.groupBoxItemMonitor.TabIndex = 0;
            this.groupBoxItemMonitor.TabStop = false;
            this.groupBoxItemMonitor.Text = "物品监控配置";

            this.lblItemList.AutoSize = true;
            this.lblItemList.Location = new Point(15, 25);
            this.lblItemList.Name = "lblItemList";
            this.lblItemList.Size = new Size(80, 17);
            this.lblItemList.TabIndex = 0;
            this.lblItemList.Text = "监控物品列表:";

            // listViewMonitoredItems
            this.listViewMonitoredItems.Location = new Point(15, 45);
            this.listViewMonitoredItems.Name = "listViewMonitoredItems";
            this.listViewMonitoredItems.Size = new Size(900, 375);
            this.listViewMonitoredItems.TabIndex = 1;
            this.listViewMonitoredItems.UseCompatibleStateImageBehavior = false;
            this.listViewMonitoredItems.View = View.Details;
            this.listViewMonitoredItems.FullRowSelect = true;
            this.listViewMonitoredItems.GridLines = true;
            this.listViewMonitoredItems.MultiSelect = false;
            this.listViewMonitoredItems.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            
            // 添加列标题 - 优化宽度，重点放宽指定字段
            this.listViewMonitoredItems.Columns.Add("物品名称", 180);        // 物品名称
            this.listViewMonitoredItems.Columns.Add("类别", 90);             // 类别
            this.listViewMonitoredItems.Columns.Add("等级", 50);             // 物品等级
            this.listViewMonitoredItems.Columns.Add("最低价格", 110);         // 价格显示
            this.listViewMonitoredItems.Columns.Add("最高价格", 110);         // 价格显示
            this.listViewMonitoredItems.Columns.Add("优先级", 60);            // 优先级数字
            this.listViewMonitoredItems.Columns.Add("状态", 60);              // 启用/禁用状态
            this.listViewMonitoredItems.Columns.Add("自动购买", 70);           // 是/否
            this.listViewMonitoredItems.Columns.Add("发现次数", 90);           // 发现次数
            this.listViewMonitoredItems.Columns.Add("购买次数", 90);           // 购买次数
            this.listViewMonitoredItems.Columns.Add("最后发现价格", 110);       // 最后发现的价格
            this.listViewMonitoredItems.Columns.Add("最后发现时间", 130);       // 最后发现时间
            
            // 双击编辑事件
            this.listViewMonitoredItems.DoubleClick += new EventHandler(this.listViewMonitoredItems_DoubleClick);
            
            // 配置右键菜单
            this.listViewMonitoredItems.ContextMenuStrip = this.contextMenuMonitoredItems;
            
            // 配置右键菜单项
            this.contextMenuMonitoredItems.Items.AddRange(new ToolStripItem[] {
                this.menuItemEdit,
                this.menuItemDelete,
                this.menuSeparator1,
                this.menuItemToggleStatus,
                this.menuItemToggleAutoBuy,
                this.menuSeparator2,
                this.menuItemRefresh
            });
            this.contextMenuMonitoredItems.Name = "contextMenuMonitoredItems";
            this.contextMenuMonitoredItems.Size = new Size(180, 150);
            this.contextMenuMonitoredItems.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuMonitoredItems_Opening);

            // 编辑菜单项
            this.menuItemEdit.Name = "menuItemEdit";
            this.menuItemEdit.Size = new Size(179, 22);
            this.menuItemEdit.Text = "✏️ 编辑物品";
            this.menuItemEdit.Click += new EventHandler(this.menuItemEdit_Click);

            // 删除菜单项
            this.menuItemDelete.Name = "menuItemDelete";
            this.menuItemDelete.Size = new Size(179, 22);
            this.menuItemDelete.Text = "🗑️ 删除物品";
            this.menuItemDelete.Click += new EventHandler(this.menuItemDelete_Click);

            // 分隔符
            this.menuSeparator1.Name = "menuSeparator1";
            this.menuSeparator1.Size = new Size(176, 6);

            // 切换状态菜单项
            this.menuItemToggleStatus.Name = "menuItemToggleStatus";
            this.menuItemToggleStatus.Size = new Size(179, 22);
            this.menuItemToggleStatus.Text = "🔄 切换启用状态";
            this.menuItemToggleStatus.Click += new EventHandler(this.menuItemToggleStatus_Click);

            // 切换自动购买菜单项
            this.menuItemToggleAutoBuy.Name = "menuItemToggleAutoBuy";
            this.menuItemToggleAutoBuy.Size = new Size(179, 22);
            this.menuItemToggleAutoBuy.Text = "🛒 切换自动购买";
            this.menuItemToggleAutoBuy.Click += new EventHandler(this.menuItemToggleAutoBuy_Click);

            // 分隔符2
            this.menuSeparator2.Name = "menuSeparator2";
            this.menuSeparator2.Size = new Size(176, 6);

            // 刷新菜单项
            this.menuItemRefresh.Name = "menuItemRefresh";
            this.menuItemRefresh.Size = new Size(179, 22);
            this.menuItemRefresh.Text = "🔄 刷新数据";
            this.menuItemRefresh.Click += new EventHandler(this.menuItemRefresh_Click);


            this.btnAddItem.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAddItem.FlatStyle = FlatStyle.Flat;
            this.btnAddItem.ForeColor = Color.White;
            this.btnAddItem.Location = new Point(15, 435);
            this.btnAddItem.Name = "btnAddItem";
            this.btnAddItem.Size = new Size(100, 30);
            this.btnAddItem.TabIndex = 4;
            this.btnAddItem.Text = "➕ 添加新物品";
            this.btnAddItem.UseVisualStyleBackColor = false;
            this.btnAddItem.Click += new EventHandler(this.btnAddItem_Click);

            this.btnRemoveItem.BackColor = Color.FromArgb(220, 53, 69);
            this.btnRemoveItem.FlatStyle = FlatStyle.Flat;
            this.btnRemoveItem.ForeColor = Color.White;
            this.btnRemoveItem.Location = new Point(125, 435);
            this.btnRemoveItem.Name = "btnRemoveItem";
            this.btnRemoveItem.Size = new Size(80, 30);
            this.btnRemoveItem.TabIndex = 5;
            this.btnRemoveItem.Text = "删除选中";
            this.btnRemoveItem.UseVisualStyleBackColor = false;
            this.btnRemoveItem.Click += new EventHandler(this.btnRemoveItem_Click);

            this.btnEditItem.BackColor = Color.FromArgb(33, 150, 243);
            this.btnEditItem.FlatStyle = FlatStyle.Flat;
            this.btnEditItem.ForeColor = Color.White;
            this.btnEditItem.Location = new Point(215, 435);
            this.btnEditItem.Name = "btnEditItem";
            this.btnEditItem.Size = new Size(80, 30);
            this.btnEditItem.TabIndex = 6;
            this.btnEditItem.Text = "编辑选中";
            this.btnEditItem.UseVisualStyleBackColor = false;
            this.btnEditItem.Click += new EventHandler(this.btnEditItem_Click);

            this.btnClearItems.BackColor = Color.FromArgb(108, 117, 125);
            this.btnClearItems.FlatStyle = FlatStyle.Flat;
            this.btnClearItems.ForeColor = Color.White;
            this.btnClearItems.Location = new Point(305, 435);
            this.btnClearItems.Name = "btnClearItems";
            this.btnClearItems.Size = new Size(80, 30);
            this.btnClearItems.TabIndex = 7;
            this.btnClearItems.Text = "清空列表";
            this.btnClearItems.UseVisualStyleBackColor = false;
            this.btnClearItems.Click += new EventHandler(this.btnClearItems_Click);

            // 刷新按钮
            this.btnRefreshItems.BackColor = Color.FromArgb(23, 162, 184);
            this.btnRefreshItems.FlatStyle = FlatStyle.Flat;
            this.btnRefreshItems.ForeColor = Color.White;
            this.btnRefreshItems.Location = new Point(395, 435);
            this.btnRefreshItems.Name = "btnRefreshItems";
            this.btnRefreshItems.Size = new Size(80, 30);
            this.btnRefreshItems.TabIndex = 8;
            this.btnRefreshItems.Text = "🔄 刷新";
            this.btnRefreshItems.UseVisualStyleBackColor = false;
            this.btnRefreshItems.Click += new EventHandler(this.btnRefreshItems_Click);


            // 交易设置
            this.groupBoxTradingSettings.Controls.Add(this.lblMaxInvestment);
            this.groupBoxTradingSettings.Controls.Add(this.textBoxMaxInvestment);
            this.groupBoxTradingSettings.Controls.Add(this.lblPriceThreshold);
            this.groupBoxTradingSettings.Controls.Add(this.trackBarPriceThreshold);
            this.groupBoxTradingSettings.Controls.Add(this.lblThresholdValue);
            this.groupBoxTradingSettings.Controls.Add(this.checkBoxAutoTrading);
            this.groupBoxTradingSettings.Location = new Point(20, 20);
            this.groupBoxTradingSettings.Name = "groupBoxTradingSettings";
            this.groupBoxTradingSettings.Size = new Size(400, 190);
            this.groupBoxTradingSettings.TabIndex = 2;
            this.groupBoxTradingSettings.TabStop = false;
            this.groupBoxTradingSettings.Text = "交易设置";

            this.lblMaxInvestment.AutoSize = true;
            this.lblMaxInvestment.Location = new Point(15, 30);
            this.lblMaxInvestment.Name = "lblMaxInvestment";
            this.lblMaxInvestment.Size = new Size(92, 17);
            this.lblMaxInvestment.TabIndex = 0;
            this.lblMaxInvestment.Text = "最大投资金额:";

            this.textBoxMaxInvestment.Location = new Point(115, 27);
            this.textBoxMaxInvestment.Name = "textBoxMaxInvestment";
            this.textBoxMaxInvestment.Size = new Size(190, 23);
            this.textBoxMaxInvestment.TabIndex = 1;
            this.textBoxMaxInvestment.Text = "1000000";

            this.lblPriceThreshold.AutoSize = true;
            this.lblPriceThreshold.Location = new Point(15, 65);
            this.lblPriceThreshold.Name = "lblPriceThreshold";
            this.lblPriceThreshold.Size = new Size(68, 17);
            this.lblPriceThreshold.TabIndex = 2;
            this.lblPriceThreshold.Text = "价格阈值:";

            this.trackBarPriceThreshold.Location = new Point(115, 62);
            this.trackBarPriceThreshold.Maximum = 100;
            this.trackBarPriceThreshold.Minimum = 10;
            this.trackBarPriceThreshold.Name = "trackBarPriceThreshold";
            this.trackBarPriceThreshold.Size = new Size(150, 45);
            this.trackBarPriceThreshold.TabIndex = 3;
            this.trackBarPriceThreshold.Value = 70;
            this.trackBarPriceThreshold.Scroll += new EventHandler(this.trackBarPriceThreshold_Scroll);

            this.lblThresholdValue.AutoSize = true;
            this.lblThresholdValue.Location = new Point(275, 65);
            this.lblThresholdValue.Name = "lblThresholdValue";
            this.lblThresholdValue.Size = new Size(30, 17);
            this.lblThresholdValue.TabIndex = 4;
            this.lblThresholdValue.Text = "0.7";

            this.checkBoxAutoTrading.AutoSize = true;
            this.checkBoxAutoTrading.Location = new Point(15, 120);
            this.checkBoxAutoTrading.Name = "checkBoxAutoTrading";
            this.checkBoxAutoTrading.Size = new Size(87, 21);
            this.checkBoxAutoTrading.TabIndex = 5;
            this.checkBoxAutoTrading.Text = "启用自动交易";
            this.checkBoxAutoTrading.UseVisualStyleBackColor = true;

            // 添加交易提醒设置
            var lblTradingNotification = new Label();
            lblTradingNotification.AutoSize = true;
            lblTradingNotification.Location = new Point(15, 150);
            lblTradingNotification.Name = "lblTradingNotification";
            lblTradingNotification.Size = new Size(80, 17);
            lblTradingNotification.TabIndex = 6;
            lblTradingNotification.Text = "交易提醒:";

            var checkBoxTradingSound = new CheckBox();
            checkBoxTradingSound.AutoSize = true;
            checkBoxTradingSound.Checked = true;
            checkBoxTradingSound.Location = new Point(100, 150);
            checkBoxTradingSound.Name = "checkBoxTradingSound";
            checkBoxTradingSound.Size = new Size(75, 21);
            checkBoxTradingSound.TabIndex = 7;
            checkBoxTradingSound.Text = "声音提醒";
            checkBoxTradingSound.UseVisualStyleBackColor = true;

            var checkBoxTradingPopup = new CheckBox();
            checkBoxTradingPopup.AutoSize = true;
            checkBoxTradingPopup.Checked = true;
            checkBoxTradingPopup.Location = new Point(180, 150);
            checkBoxTradingPopup.Name = "checkBoxTradingPopup";
            checkBoxTradingPopup.Size = new Size(75, 21);
            checkBoxTradingPopup.TabIndex = 8;
            checkBoxTradingPopup.Text = "弹窗提醒";
            checkBoxTradingPopup.UseVisualStyleBackColor = true;

            this.groupBoxTradingSettings.Controls.Add(lblTradingNotification);
            this.groupBoxTradingSettings.Controls.Add(checkBoxTradingSound);
            this.groupBoxTradingSettings.Controls.Add(checkBoxTradingPopup);

            // 安全设置
            this.groupBoxSafetySettings.Controls.Add(this.checkBoxHumanBehavior);
            this.groupBoxSafetySettings.Controls.Add(this.lblOperationInterval);
            this.groupBoxSafetySettings.Controls.Add(this.textBoxMinInterval);
            this.groupBoxSafetySettings.Controls.Add(this.textBoxMaxInterval);
            this.groupBoxSafetySettings.Controls.Add(this.lblIntervalUnit);
            this.groupBoxSafetySettings.Location = new Point(440, 20);
            this.groupBoxSafetySettings.Name = "groupBoxSafetySettings";
            this.groupBoxSafetySettings.Size = new Size(400, 120);
            this.groupBoxSafetySettings.TabIndex = 3;
            this.groupBoxSafetySettings.TabStop = false;
            this.groupBoxSafetySettings.Text = "安全设置";

            this.checkBoxHumanBehavior.AutoSize = true;
            this.checkBoxHumanBehavior.Checked = true;
            this.checkBoxHumanBehavior.CheckState = CheckState.Checked;
            this.checkBoxHumanBehavior.Location = new Point(15, 30);
            this.checkBoxHumanBehavior.Name = "checkBoxHumanBehavior";
            this.checkBoxHumanBehavior.Size = new Size(123, 21);
            this.checkBoxHumanBehavior.TabIndex = 0;
            this.checkBoxHumanBehavior.Text = "启用人性化操作";
            this.checkBoxHumanBehavior.UseVisualStyleBackColor = true;

            this.lblOperationInterval.AutoSize = true;
            this.lblOperationInterval.Location = new Point(15, 65);
            this.lblOperationInterval.Name = "lblOperationInterval";
            this.lblOperationInterval.Size = new Size(92, 17);
            this.lblOperationInterval.TabIndex = 1;
            this.lblOperationInterval.Text = "操作间隔(秒):";

            this.textBoxMinInterval.Location = new Point(115, 62);
            this.textBoxMinInterval.Name = "textBoxMinInterval";
            this.textBoxMinInterval.Size = new Size(60, 23);
            this.textBoxMinInterval.TabIndex = 2;
            this.textBoxMinInterval.Text = "3";

            this.textBoxMaxInterval.Location = new Point(200, 62);
            this.textBoxMaxInterval.Name = "textBoxMaxInterval";
            this.textBoxMaxInterval.Size = new Size(60, 23);
            this.textBoxMaxInterval.TabIndex = 3;
            this.textBoxMaxInterval.Text = "8";

            this.lblIntervalUnit.AutoSize = true;
            this.lblIntervalUnit.Location = new Point(180, 65);
            this.lblIntervalUnit.Name = "lblIntervalUnit";
            this.lblIntervalUnit.Size = new Size(15, 17);
            this.lblIntervalUnit.TabIndex = 4;
            this.lblIntervalUnit.Text = "~";

            // 保存按钮
            this.btnSaveSettings.BackColor = Color.FromArgb(40, 167, 69);
            this.btnSaveSettings.FlatStyle = FlatStyle.Flat;
            this.btnSaveSettings.ForeColor = Color.White;
            this.btnSaveSettings.Location = new Point(380, 220);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new Size(100, 40);
            this.btnSaveSettings.TabIndex = 4;
            this.btnSaveSettings.Text = "💾 保存交易设置";
            this.btnSaveSettings.UseVisualStyleBackColor = false;
            this.btnSaveSettings.Click += new EventHandler(this.btnSaveSettings_Click);

            // 示例数据将通过代码动态添加到ListView

            // 将控件添加到监控设置页面
            this.tabPageSettings.Controls.Add(this.groupBoxItemMonitor);

            // 将交易设置添加到交易设置页面
            this.tabPageTrading.Controls.Add(this.groupBoxTradingSettings);
            this.tabPageTrading.Controls.Add(this.groupBoxSafetySettings);
            this.tabPageTrading.Controls.Add(this.btnSaveSettings);

            // 恢复布局
            this.groupBoxItemMonitor.ResumeLayout(false);
            this.groupBoxTradingSettings.ResumeLayout(false);
            this.groupBoxSafetySettings.ResumeLayout(false);
        }

        private void InitializeHistoryControls()
        {
            // 初始化交易历史控件
            this.groupBoxHistoryFilter = new GroupBox();
            this.lblHistoryDateRange = new Label();
            this.dateTimePickerStart = new DateTimePicker();
            this.lblHistoryDateTo = new Label();
            this.dateTimePickerEnd = new DateTimePicker();
            this.lblHistoryStatus = new Label();
            this.comboBoxHistoryStatus = new ComboBox();
            this.lblHistoryItemName = new Label();
            this.textBoxHistoryItemName = new TextBox();
            this.btnHistorySearch = new Button();
            this.btnHistoryReset = new Button();
            
            this.groupBoxHistoryList = new GroupBox();
            this.listViewHistory = new ListView();
            this.lblHistoryStats = new Label();
            this.lblHistoryPagination = new Label();
            this.btnHistoryFirst = new Button();
            this.btnHistoryPrev = new Button();
            this.lblHistoryCurrentPage = new Label();
            this.btnHistoryNext = new Button();
            this.btnHistoryLast = new Button();
            this.lblHistoryPageSize = new Label();
            this.comboBoxHistoryPageSize = new ComboBox();

            // 暂停布局
            this.groupBoxHistoryFilter.SuspendLayout();
            this.groupBoxHistoryList.SuspendLayout();

            // 筛选条件组
            this.groupBoxHistoryFilter.Controls.Add(this.lblHistoryDateRange);
            this.groupBoxHistoryFilter.Controls.Add(this.dateTimePickerStart);
            this.groupBoxHistoryFilter.Controls.Add(this.lblHistoryDateTo);
            this.groupBoxHistoryFilter.Controls.Add(this.dateTimePickerEnd);
            this.groupBoxHistoryFilter.Controls.Add(this.lblHistoryStatus);
            this.groupBoxHistoryFilter.Controls.Add(this.comboBoxHistoryStatus);
            this.groupBoxHistoryFilter.Controls.Add(this.lblHistoryItemName);
            this.groupBoxHistoryFilter.Controls.Add(this.textBoxHistoryItemName);
            this.groupBoxHistoryFilter.Controls.Add(this.btnHistorySearch);
            this.groupBoxHistoryFilter.Controls.Add(this.btnHistoryReset);
            
            this.groupBoxHistoryFilter.Location = new Point(10, 10);
            this.groupBoxHistoryFilter.Name = "groupBoxHistoryFilter";
            this.groupBoxHistoryFilter.Size = new Size(1410, 100);
            this.groupBoxHistoryFilter.TabIndex = 0;
            this.groupBoxHistoryFilter.TabStop = false;
            this.groupBoxHistoryFilter.Text = "筛选条件";

            // 日期范围标签
            this.lblHistoryDateRange.AutoSize = true;
            this.lblHistoryDateRange.Location = new Point(15, 30);
            this.lblHistoryDateRange.Name = "lblHistoryDateRange";
            this.lblHistoryDateRange.Size = new Size(68, 17);
            this.lblHistoryDateRange.TabIndex = 0;
            this.lblHistoryDateRange.Text = "日期范围:";

            // 开始日期
            this.dateTimePickerStart.Format = DateTimePickerFormat.Short;
            this.dateTimePickerStart.Location = new Point(90, 27);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new Size(120, 23);
            this.dateTimePickerStart.TabIndex = 1;
            this.dateTimePickerStart.Value = DateTime.Now.AddDays(-30); // 默认显示最近30天

            // 到
            this.lblHistoryDateTo.AutoSize = true;
            this.lblHistoryDateTo.Location = new Point(220, 30);
            this.lblHistoryDateTo.Name = "lblHistoryDateTo";
            this.lblHistoryDateTo.Size = new Size(20, 17);
            this.lblHistoryDateTo.TabIndex = 2;
            this.lblHistoryDateTo.Text = "到";

            // 结束日期
            this.dateTimePickerEnd.Format = DateTimePickerFormat.Short;
            this.dateTimePickerEnd.Location = new Point(250, 27);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new Size(120, 23);
            this.dateTimePickerEnd.TabIndex = 3;
            this.dateTimePickerEnd.Value = DateTime.Now;

            // 状态标签
            this.lblHistoryStatus.AutoSize = true;
            this.lblHistoryStatus.Location = new Point(390, 30);
            this.lblHistoryStatus.Name = "lblHistoryStatus";
            this.lblHistoryStatus.Size = new Size(44, 17);
            this.lblHistoryStatus.TabIndex = 4;
            this.lblHistoryStatus.Text = "状态:";

            // 状态下拉框
            this.comboBoxHistoryStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxHistoryStatus.Location = new Point(440, 27);
            this.comboBoxHistoryStatus.Name = "comboBoxHistoryStatus";
            this.comboBoxHistoryStatus.Size = new Size(100, 25);
            this.comboBoxHistoryStatus.TabIndex = 5;
            this.comboBoxHistoryStatus.Items.AddRange(new object[] { "全部", "待处理", "执行中", "已完成", "失败", "已取消" });
            this.comboBoxHistoryStatus.SelectedIndex = 0;

            // 物品名称标签
            this.lblHistoryItemName.AutoSize = true;
            this.lblHistoryItemName.Location = new Point(560, 30);
            this.lblHistoryItemName.Name = "lblHistoryItemName";
            this.lblHistoryItemName.Size = new Size(68, 17);
            this.lblHistoryItemName.TabIndex = 6;
            this.lblHistoryItemName.Text = "物品名称:";

            // 物品名称输入框
            this.textBoxHistoryItemName.Location = new Point(635, 27);
            this.textBoxHistoryItemName.Name = "textBoxHistoryItemName";
            this.textBoxHistoryItemName.Size = new Size(150, 23);
            this.textBoxHistoryItemName.TabIndex = 7;
            this.textBoxHistoryItemName.PlaceholderText = "输入物品名称搜索";

            // 搜索按钮
            this.btnHistorySearch.BackColor = Color.FromArgb(0, 123, 255);
            this.btnHistorySearch.FlatStyle = FlatStyle.Flat;
            this.btnHistorySearch.ForeColor = Color.White;
            this.btnHistorySearch.Location = new Point(800, 25);
            this.btnHistorySearch.Name = "btnHistorySearch";
            this.btnHistorySearch.Size = new Size(80, 30);
            this.btnHistorySearch.TabIndex = 8;
            this.btnHistorySearch.Text = "🔍 搜索";
            this.btnHistorySearch.UseVisualStyleBackColor = false;

            // 重置按钮
            this.btnHistoryReset.BackColor = Color.FromArgb(108, 117, 125);
            this.btnHistoryReset.FlatStyle = FlatStyle.Flat;
            this.btnHistoryReset.ForeColor = Color.White;
            this.btnHistoryReset.Location = new Point(890, 25);
            this.btnHistoryReset.Name = "btnHistoryReset";
            this.btnHistoryReset.Size = new Size(80, 30);
            this.btnHistoryReset.TabIndex = 9;
            this.btnHistoryReset.Text = "🔄 重置";
            this.btnHistoryReset.UseVisualStyleBackColor = false;

            // 添加测试数据按钮
            this.btnAddTestData = new Button();
            this.btnAddTestData.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAddTestData.FlatStyle = FlatStyle.Flat;
            this.btnAddTestData.ForeColor = Color.White;
            this.btnAddTestData.Location = new Point(980, 25);
            this.btnAddTestData.Name = "btnAddTestData";
            this.btnAddTestData.Size = new Size(100, 30);
            this.btnAddTestData.TabIndex = 10;
            this.btnAddTestData.Text = "📊 添加测试数据";
            this.btnAddTestData.UseVisualStyleBackColor = false;
            this.groupBoxHistoryFilter.Controls.Add(this.btnAddTestData);

            // 交易历史列表组
            this.groupBoxHistoryList.Controls.Add(this.listViewHistory);
            this.groupBoxHistoryList.Controls.Add(this.lblHistoryStats);
            this.groupBoxHistoryList.Controls.Add(this.lblHistoryPagination);
            this.groupBoxHistoryList.Controls.Add(this.btnHistoryFirst);
            this.groupBoxHistoryList.Controls.Add(this.btnHistoryPrev);
            this.groupBoxHistoryList.Controls.Add(this.lblHistoryCurrentPage);
            this.groupBoxHistoryList.Controls.Add(this.btnHistoryNext);
            this.groupBoxHistoryList.Controls.Add(this.btnHistoryLast);
            this.groupBoxHistoryList.Controls.Add(this.lblHistoryPageSize);
            this.groupBoxHistoryList.Controls.Add(this.comboBoxHistoryPageSize);
            
            this.groupBoxHistoryList.Location = new Point(10, 120);
            this.groupBoxHistoryList.Name = "groupBoxHistoryList";
            this.groupBoxHistoryList.Size = new Size(1410, 680);
            this.groupBoxHistoryList.TabIndex = 1;
            this.groupBoxHistoryList.TabStop = false;
            this.groupBoxHistoryList.Text = "交易历史记录";

            // 交易历史ListView
            this.listViewHistory.Location = new Point(15, 45);
            this.listViewHistory.Name = "listViewHistory";
            this.listViewHistory.Size = new Size(1380, 560);
            this.listViewHistory.TabIndex = 0;
            this.listViewHistory.UseCompatibleStateImageBehavior = false;
            this.listViewHistory.View = View.Details;
            this.listViewHistory.FullRowSelect = true;
            this.listViewHistory.GridLines = true;
            this.listViewHistory.MultiSelect = false;
            this.listViewHistory.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            
            // 添加列标题
            this.listViewHistory.Columns.Add("购买时间", 220);
            this.listViewHistory.Columns.Add("物品名称", 180);
            this.listViewHistory.Columns.Add("购买价格", 100);
            this.listViewHistory.Columns.Add("数量", 60);
            this.listViewHistory.Columns.Add("总金额", 100);
            this.listViewHistory.Columns.Add("卖家", 120);
            this.listViewHistory.Columns.Add("预期利润", 100);
            this.listViewHistory.Columns.Add("实际利润", 100);
            this.listViewHistory.Columns.Add("策略", 80);
            this.listViewHistory.Columns.Add("状态", 80);
            this.listViewHistory.Columns.Add("执行时间(ms)", 100);
            this.listViewHistory.Columns.Add("备注", 180);

            // 统计信息标签
            this.lblHistoryStats.AutoSize = true;
            this.lblHistoryStats.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            this.lblHistoryStats.ForeColor = Color.FromArgb(0, 123, 255);
            this.lblHistoryStats.Location = new Point(15, 615);
            this.lblHistoryStats.Name = "lblHistoryStats";
            this.lblHistoryStats.Size = new Size(200, 17);
            this.lblHistoryStats.TabIndex = 1;
            this.lblHistoryStats.Text = "总记录: 0 | 成功: 0 | 失败: 0";

            // 分页标签
            this.lblHistoryPagination.AutoSize = true;
            this.lblHistoryPagination.Location = new Point(15, 645);
            this.lblHistoryPagination.Name = "lblHistoryPagination";
            this.lblHistoryPagination.Size = new Size(44, 17);
            this.lblHistoryPagination.TabIndex = 2;
            this.lblHistoryPagination.Text = "分页:";

            // 首页按钮
            this.btnHistoryFirst.Location = new Point(65, 642);
            this.btnHistoryFirst.Name = "btnHistoryFirst";
            this.btnHistoryFirst.Size = new Size(50, 25);
            this.btnHistoryFirst.TabIndex = 3;
            this.btnHistoryFirst.Text = "首页";
            this.btnHistoryFirst.UseVisualStyleBackColor = true;

            // 上一页按钮
            this.btnHistoryPrev.Location = new Point(125, 642);
            this.btnHistoryPrev.Name = "btnHistoryPrev";
            this.btnHistoryPrev.Size = new Size(60, 25);
            this.btnHistoryPrev.TabIndex = 4;
            this.btnHistoryPrev.Text = "上一页";
            this.btnHistoryPrev.UseVisualStyleBackColor = true;

            // 当前页标签
            this.lblHistoryCurrentPage.AutoSize = true;
            this.lblHistoryCurrentPage.Location = new Point(195, 647);
            this.lblHistoryCurrentPage.Name = "lblHistoryCurrentPage";
            this.lblHistoryCurrentPage.Size = new Size(80, 17);
            this.lblHistoryCurrentPage.TabIndex = 5;
            this.lblHistoryCurrentPage.Text = "第 1 页，共 1 页";

            // 下一页按钮
            this.btnHistoryNext.Location = new Point(285, 642);
            this.btnHistoryNext.Name = "btnHistoryNext";
            this.btnHistoryNext.Size = new Size(60, 25);
            this.btnHistoryNext.TabIndex = 6;
            this.btnHistoryNext.Text = "下一页";
            this.btnHistoryNext.UseVisualStyleBackColor = true;

            // 末页按钮
            this.btnHistoryLast.Location = new Point(355, 642);
            this.btnHistoryLast.Name = "btnHistoryLast";
            this.btnHistoryLast.Size = new Size(50, 25);
            this.btnHistoryLast.TabIndex = 7;
            this.btnHistoryLast.Text = "末页";
            this.btnHistoryLast.UseVisualStyleBackColor = true;

            // 每页显示数量标签
            this.lblHistoryPageSize.AutoSize = true;
            this.lblHistoryPageSize.Location = new Point(420, 647);
            this.lblHistoryPageSize.Name = "lblHistoryPageSize";
            this.lblHistoryPageSize.Size = new Size(80, 17);
            this.lblHistoryPageSize.TabIndex = 8;
            this.lblHistoryPageSize.Text = "每页显示:";

            // 每页显示数量下拉框
            this.comboBoxHistoryPageSize.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxHistoryPageSize.Location = new Point(505, 644);
            this.comboBoxHistoryPageSize.Name = "comboBoxHistoryPageSize";
            this.comboBoxHistoryPageSize.Size = new Size(80, 25);
            this.comboBoxHistoryPageSize.TabIndex = 9;
            this.comboBoxHistoryPageSize.Items.AddRange(new object[] { "10", "20", "50", "100" });
            this.comboBoxHistoryPageSize.SelectedIndex = 1; // 默认20条

            // 将控件添加到交易历史页面
            this.tabPageHistory.Controls.Add(this.groupBoxHistoryFilter);
            this.tabPageHistory.Controls.Add(this.groupBoxHistoryList);

            // 恢复布局
            this.groupBoxHistoryFilter.ResumeLayout(false);
            this.groupBoxHistoryList.ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPageMonitor;
        private TabPage tabPageSettings;
        private TabPage tabPageTrading;
        private TabPage tabPageHistory;
        private GroupBox groupBoxControl;
        private Button btnStart;
        private Button btnStop;
        private Button btnSettings;
        private Button btnTestDetection;
        private GroupBox groupBoxStats;
        private Label lblMonitoredItems;
        private Label lblOpportunities;
        private Label lblExecutedTrades;
        private Label lblTotalProfit;
        private ListView listViewOpportunities;
        private TextBox textBoxLog;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        
        // 设置页面控件
        private GroupBox groupBoxItemMonitor;
        private ListView listViewMonitoredItems;
        private ContextMenuStrip contextMenuMonitoredItems;
        private ToolStripMenuItem menuItemEdit;
        private ToolStripMenuItem menuItemDelete;
        private ToolStripSeparator menuSeparator1;
        private ToolStripMenuItem menuItemToggleStatus;
        private ToolStripMenuItem menuItemToggleAutoBuy;
        private ToolStripSeparator menuSeparator2;
        private ToolStripMenuItem menuItemRefresh;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Button btnClearItems;
        private Button btnEditItem;
        private Button btnRefreshItems;
        private Label lblItemList;
        
        
        private GroupBox groupBoxTradingSettings;
        private Label lblMaxInvestment;
        private TextBox textBoxMaxInvestment;
        private Label lblPriceThreshold;
        private TrackBar trackBarPriceThreshold;
        private Label lblThresholdValue;
        private CheckBox checkBoxAutoTrading;
        
        private GroupBox groupBoxSafetySettings;
        private CheckBox checkBoxHumanBehavior;
        private Label lblOperationInterval;
        private TextBox textBoxMinInterval;
        private TextBox textBoxMaxInterval;
        private Label lblIntervalUnit;
        
        private Button btnSaveSettings;
        
        // 交易历史页面控件
        private GroupBox groupBoxHistoryFilter;
        private Label lblHistoryDateRange;
        private DateTimePicker dateTimePickerStart;
        private Label lblHistoryDateTo;
        private DateTimePicker dateTimePickerEnd;
        private Label lblHistoryStatus;
        private ComboBox comboBoxHistoryStatus;
        private Label lblHistoryItemName;
        private TextBox textBoxHistoryItemName;
        private Button btnHistorySearch;
        private Button btnHistoryReset;
        private Button btnAddTestData;
        
        private GroupBox groupBoxHistoryList;
        private ListView listViewHistory;
        private Label lblHistoryStats;
        private Label lblHistoryPagination;
        private Button btnHistoryFirst;
        private Button btnHistoryPrev;
        private Label lblHistoryCurrentPage;
        private Button btnHistoryNext;
        private Button btnHistoryLast;
        private Label lblHistoryPageSize;
        private ComboBox comboBoxHistoryPageSize;
    }
}
