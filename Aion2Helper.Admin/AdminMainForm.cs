using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Services;
using Aion2Helper.Models;

namespace Aion2Helper.Admin
{
    /// <summary>
    /// 管理端主窗体
    /// </summary>
    public partial class AdminMainForm : Form
    {
        private TabControl tabControl;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblDBStatus;
        
        // 性能优化：缓存机器名称映射
        private Dictionary<string, string> _machineDisplayNameCache = new Dictionary<string, string>();
        
        // 监控物品管理页
        private DataGridView dgvMonitoredItems;
        private Button btnRefreshMonitoredItems;
        private Button btnAddMonitoredItem;
        private Button btnEditMonitoredItem;
        private Button btnDeleteMonitoredItem;
        private Button btnSearchMonitoredItems;
        private ComboBox cboMachineCodeFilter;
        private ComboBox cboMonitoredCategoryFilter;
        private ComboBox cboMonitoredItemFilter;
        private TextBox txtSearchMonitoredItem;
        private Label lblTotalItems;
        
        // 购买记录页
        private DataGridView dgvPurchaseRecords;
        private Button btnRefreshPurchaseRecords;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cboMachineFilter;
        private Label lblTotalRecords;
        
        // 统计分析页
        private Label lblTotalMachines;
        private Label lblTotalPurchases;
        private Label lblTotalProfit;
        private Button btnRefreshStats;

        // 物品管理页
        private DataGridView dgvItems;
        private Button btnRefreshItems;
        private Button btnAddItem;
        private Button btnEditItem;
        private Button btnDeleteItem;
        private TextBox txtSearchItemName;
        private ComboBox cboCategoryFilter;
        private Label lblTotalItemsCount;

        public AdminMainForm()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void InitializeComponent()
        {
            this.Text = "Aion2 拍卖行管理系统";
            this.Size = new Size(2100, 1350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1800, 1050);

            // 创建TabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // 状态栏
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("就绪");
            lblDBStatus = new ToolStripStatusLabel("数据库: 未连接");
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblDBStatus });

            this.Controls.Add(tabControl);
            this.Controls.Add(statusStrip);

            CreateMonitoredItemsTab();
            CreatePurchaseRecordsTab();
            CreateStatisticsTab();
            CreateItemsTab();
            CreateAIManagementTab();
        }

        private void CreateMonitoredItemsTab()
        {
            var tabPage = new TabPage("监控物品管理");
            
            // 工具栏
            var toolPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(15) };
            
            // 第一行：筛选条件
            var lblMachine = new Label { Text = "机器:", Location = new Point(15, 20), AutoSize = true };
            cboMachineCodeFilter = new ComboBox { Location = new Point(70, 17), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMachineCodeFilter.SelectedIndexChanged += CboMachineCodeFilter_SelectedIndexChanged;
            
            var lblCategory = new Label { Text = "分类:", Location = new Point(290, 20), AutoSize = true };
            cboMonitoredCategoryFilter = new ComboBox { Location = new Point(345, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMonitoredCategoryFilter.SelectedIndexChanged += CboMonitoredCategoryFilter_SelectedIndexChanged;
            
            var lblItem = new Label { Text = "物品:", Location = new Point(515, 20), AutoSize = true };
            cboMonitoredItemFilter = new ComboBox { Location = new Point(570, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMonitoredItemFilter.SelectedIndexChanged += CboMonitoredItemFilter_SelectedIndexChanged;
            
            var lblSearch = new Label { Text = "搜索:", Location = new Point(840, 20), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10) };
            txtSearchMonitoredItem = new TextBox { Location = new Point(895, 17), Width = 280, Height = 28, Font = new Font("Microsoft YaHei UI", 10), PlaceholderText = "输入物品名称搜索..." };
            btnSearchMonitoredItems = new Button { Text = "🔍 搜索", Location = new Point(1190, 13), Size = new Size(100, 40), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold) };
            btnSearchMonitoredItems.Click += BtnSearchMonitoredItems_Click;
            
            // 第二行：操作按钮
            btnRefreshMonitoredItems = new Button { Text = "🔄 刷新", Location = new Point(15, 60), Size = new Size(100, 35) };
            btnAddMonitoredItem = new Button { Text = "➕ 添加", Location = new Point(130, 60), Size = new Size(100, 35), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White };
            btnEditMonitoredItem = new Button { Text = "✏️ 编辑", Location = new Point(245, 60), Size = new Size(100, 35), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White };
            btnDeleteMonitoredItem = new Button { Text = "🗑️ 删除", Location = new Point(360, 60), Size = new Size(100, 35), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White };
            lblTotalItems = new Label { Text = "总计: 0 项", Location = new Point(480, 68), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold) };
            
            btnRefreshMonitoredItems.Click += BtnRefreshMonitoredItems_Click;
            btnAddMonitoredItem.Click += BtnAddMonitoredItem_Click;
            btnEditMonitoredItem.Click += BtnEditMonitoredItem_Click;
            btnDeleteMonitoredItem.Click += BtnDeleteMonitoredItem_Click;
            
            toolPanel.Controls.AddRange(new Control[] { 
                lblMachine, cboMachineCodeFilter, lblCategory, cboMonitoredCategoryFilter, lblItem, cboMonitoredItemFilter,
                lblSearch, txtSearchMonitoredItem, btnSearchMonitoredItems,
                btnRefreshMonitoredItems, btnAddMonitoredItem, btnEditMonitoredItem, btnDeleteMonitoredItem, lblTotalItems 
            });
            
            // 数据表格
            dgvMonitoredItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersWidth = 60,
                Font = new Font("Microsoft YaHei UI", 10F),
                RowTemplate = { Height = 32 },
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 240, 240),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(5, 0, 5, 0)
                }
            };
            
            // 性能优化：启用双缓冲
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvMonitoredItems, new object[] { true });
            
            dgvMonitoredItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "物品名称", DataPropertyName = "ItemName", Width = 220 },
                new DataGridViewTextBoxColumn { HeaderText = "分类", Name = "MonitoredItemCategoryColumn", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "物品等级", DataPropertyName = "ItemLevel", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "最低价格", DataPropertyName = "TargetMinPrice", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { HeaderText = "最高价格", DataPropertyName = "TargetMaxPrice", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { HeaderText = "优先级", DataPropertyName = "Priority", Width = 90 },
                new DataGridViewCheckBoxColumn { HeaderText = "启用", DataPropertyName = "IsEnabled", Width = 80 },
                new DataGridViewCheckBoxColumn { HeaderText = "自动购买", DataPropertyName = "AutoPurchaseEnabled", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "监控策略", DataPropertyName = "MonitorStrategy", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "备注", DataPropertyName = "Notes", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "机器码", Name = "MachineCodeColumn", Width = 280 },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreatedAt", Width = 180, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" } },
                new DataGridViewTextBoxColumn { HeaderText = "更新时间", DataPropertyName = "UpdatedAt", Width = 180, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" } }
            });
            
            // 处理分类列的显示
            dgvMonitoredItems.CellFormatting += DgvMonitoredItems_CellFormatting;
            
            tabPage.Controls.Add(dgvMonitoredItems);
            tabPage.Controls.Add(toolPanel);
            tabControl.TabPages.Add(tabPage);
        }

        private void DgvMonitoredItems_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            var monitoredItem = dgvMonitoredItems.Rows[e.RowIndex].DataBoundItem as MonitoredItem;
            if (monitoredItem == null) return;
            
            // 格式化分类列
            if (dgvMonitoredItems.Columns[e.ColumnIndex].Name == "MonitoredItemCategoryColumn")
            {
                if (monitoredItem.ItemCategory != null)
                {
                    e.Value = monitoredItem.ItemCategory.Name;
                    e.FormattingApplied = true;
                }
#pragma warning disable CS0618
                else if (!string.IsNullOrEmpty(monitoredItem.Category))
                {
                    e.Value = monitoredItem.Category;
                    e.FormattingApplied = true;
                }
#pragma warning restore CS0618
            }
            
            // 格式化机器码列（显示机器名称）
            if (dgvMonitoredItems.Columns[e.ColumnIndex].Name == "MachineCodeColumn")
            {
                e.Value = GetMachineDisplayName(monitoredItem.MachineCode);
                e.FormattingApplied = true;
            }
        }
        
        /// <summary>
        /// 获取机器显示名称（使用缓存，高性能）
        /// </summary>
        private string GetMachineDisplayName(string machineCode)
        {
            if (string.IsNullOrEmpty(machineCode))
                return string.Empty;
                
            // 从缓存中获取
            if (_machineDisplayNameCache.TryGetValue(machineCode, out var displayName))
            {
                return displayName;
            }
            
            // 缓存中没有，返回默认格式并添加到缓存
            var defaultName = machineCode.Length > 16 ? $"{machineCode.Substring(0, 16)}..." : machineCode;
            _machineDisplayNameCache[machineCode] = defaultName;
            return defaultName;
        }
        
        /// <summary>
        /// 批量加载机器名称到缓存（性能优化）
        /// </summary>
        private async Task LoadMachineDisplayNameCacheAsync()
        {
            try
            {
                _machineDisplayNameCache.Clear();
                
                var cache = CacheService.Instance;
                var machines = cache.GetEnabledMachines();
                
                foreach (var machine in machines)
                {
                    var displayName = string.IsNullOrEmpty(machine.MachineName)
                        ? (machine.MachineCode.Length > 16 ? $"{machine.MachineCode.Substring(0, 16)}..." : machine.MachineCode)
                        : $"{machine.MachineName} ({machine.MachineCode.Substring(0, 8)}...)";
                    
                    _machineDisplayNameCache[machine.MachineCode] = displayName;
                }
                
                #if DEBUG
                Console.WriteLine($"[性能优化] 已加载 {_machineDisplayNameCache.Count} 个机器名称到缓存");
                #endif
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载机器名称缓存失败: {ex.Message}");
            }
        }

        private void CreatePurchaseRecordsTab()
        {
            var tabPage = new TabPage("购买记录");
            
            // 工具栏
            var toolPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            
            var lblStart = new Label { Text = "开始日期:", Location = new Point(15, 30), AutoSize = true };
            dtpStartDate = new DateTimePicker { Location = new Point(90, 25), Width = 150, Value = DateTime.Now.AddDays(-7) };
            var lblEnd = new Label { Text = "结束日期:", Location = new Point(260, 30), AutoSize = true };
            dtpEndDate = new DateTimePicker { Location = new Point(335, 25), Width = 150, Value = DateTime.Now };
            var lblMachine = new Label { Text = "机器筛选:", Location = new Point(505, 30), AutoSize = true };
            cboMachineFilter = new ComboBox { Location = new Point(580, 25), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMachineFilter.Items.Add("所有机器");
            cboMachineFilter.SelectedIndex = 0;
            btnRefreshPurchaseRecords = new Button { Text = "🔄 刷新", Location = new Point(850, 23), Size = new Size(100, 30) };
            lblTotalRecords = new Label { Text = "总计: 0 条", Location = new Point(970, 30), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold) };
            
            btnRefreshPurchaseRecords.Click += BtnRefreshPurchaseRecords_Click;
            
            toolPanel.Controls.AddRange(new Control[] { lblStart, dtpStartDate, lblEnd, dtpEndDate, lblMachine, cboMachineFilter, btnRefreshPurchaseRecords, lblTotalRecords });
            
            // 数据表格
            dgvPurchaseRecords = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersWidth = 60,
                Font = new Font("Microsoft YaHei UI", 10F),
                RowTemplate = { Height = 32 },
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 240, 240),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(5, 0, 5, 0)
                }
            };
            
            // 性能优化：启用双缓冲
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvPurchaseRecords, new object[] { true });
            
            dgvPurchaseRecords.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "物品名称", DataPropertyName = "ItemName", Width = 250 },
                new DataGridViewTextBoxColumn { HeaderText = "数量", DataPropertyName = "Quantity", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "单价", DataPropertyName = "Price", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { HeaderText = "总额", DataPropertyName = "TotalAmount", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { HeaderText = "状态", DataPropertyName = "Status", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "机器码", DataPropertyName = "MachineCode", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 350 },
                new DataGridViewTextBoxColumn { HeaderText = "购买时间", DataPropertyName = "PurchaseTime", Width = 200, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" } }
            });
            
            tabPage.Controls.Add(dgvPurchaseRecords);
            tabPage.Controls.Add(toolPanel);
            tabControl.TabPages.Add(tabPage);
        }

        private void CreateStatisticsTab()
        {
            var tabPage = new TabPage("统计分析");
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            
            // 统计卡片
            var cardPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 30),
                Size = new Size(2000, 300),
                FlowDirection = FlowDirection.LeftToRight
            };
            
            // 机器数量卡片
            var machineCard = CreateStatCard("活跃机器", "0", Color.FromArgb(0, 123, 255));
            lblTotalMachines = (Label)machineCard.Controls[1];
            
            // 总购买次数卡片
            var purchaseCard = CreateStatCard("总购买次数", "0", Color.FromArgb(40, 167, 69));
            lblTotalPurchases = (Label)purchaseCard.Controls[1];
            
            // 总利润卡片
            var profitCard = CreateStatCard("总利润", "0", Color.FromArgb(255, 193, 7));
            lblTotalProfit = (Label)profitCard.Controls[1];
            
            cardPanel.Controls.AddRange(new Control[] { machineCard, purchaseCard, profitCard });
            
            btnRefreshStats = new Button
            {
                Text = "🔄 刷新统计",
                Location = new Point(30, 360),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold)
            };
            btnRefreshStats.Click += BtnRefreshStats_Click;
            
            panel.Controls.Add(cardPanel);
            panel.Controls.Add(btnRefreshStats);
            tabPage.Controls.Add(panel);
            tabControl.TabPages.Add(tabPage);
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            var card = new Panel
            {
                Size = new Size(380, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(15)
            };
            
            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 16, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            
            var lblValue = new Label
            {
                Text = value,
                Location = new Point(20, 90),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 32, FontStyle.Bold),
                ForeColor = color
            };
            
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            
            return card;
        }

        private async void LoadInitialData()
        {
            try
            {
                lblStatus.Text = "正在连接数据库...";
                
                // 测试数据库连接
                using var context = new Aion2DbContext();
                var canConnect = await context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    lblDBStatus.Text = "数据库: ❌ 连接失败";
                    lblDBStatus.ForeColor = Color.Red;
                    lblStatus.Text = "数据库连接失败";
                    MessageBox.Show("无法连接到数据库，请检查配置！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                lblDBStatus.Text = "数据库: ✅ 已连接";
                lblDBStatus.ForeColor = Color.Green;
                
                // 后台异步初始化缓存，避免阻塞UI
                lblStatus.Text = "正在初始化缓存...";
                var cache = CacheService.Instance;
                var initSuccess = await cache.InitializeAsync();
                
                if (initSuccess)
                {
                    lblStatus.Text = "缓存初始化完成";
                    #if DEBUG
                    Console.WriteLine($"[管理端] {cache.GetCacheStats()}");
                    #endif
                    
                    // 性能优化：预加载机器名称缓存
                    lblStatus.Text = "正在加载机器名称缓存...";
                    await LoadMachineDisplayNameCacheAsync();
                    
                    // 加载筛选器数据（从缓存）
                    await LoadMachineCodesAsync();
                    await LoadMonitoredCategoriesAsync();
                    await LoadItemCategoriesAsync();
                    
                    // 加载初始数据
                    lblStatus.Text = "正在加载数据...";
                    await LoadMonitoredItemsAsync();
                    await LoadPurchaseRecordsAsync();
                    await LoadStatisticsAsync();
                    await LoadItemsAsync();
                    
                    lblStatus.Text = "就绪";
                }
                else
                {
                    lblStatus.Text = "缓存初始化失败";
                    MessageBox.Show("缓存初始化失败，部分功能可能不可用！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                lblDBStatus.Text = "数据库: ❌ 错误";
                lblDBStatus.ForeColor = Color.Red;
                lblStatus.Text = "初始化失败";
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 加载机器码列表（从缓存）
        /// </summary>
        private async Task LoadMachineCodesAsync()
        {
            try
            {
                var cache = CacheService.Instance;
                var machines = cache.GetEnabledMachines();
                
                cboMachineCodeFilter.Items.Clear();
                cboMachineCodeFilter.Items.Add("所有机器");
                
                foreach (var machine in machines)
                {
                    var displayName = string.IsNullOrEmpty(machine.MachineName)
                        ? machine.MachineCode
                        : $"{machine.MachineName} ({machine.MachineCode.Substring(0, 8)}...)";
                    cboMachineCodeFilter.Items.Add(new MachineCodeItem 
                    { 
                        DisplayName = displayName,
                        MachineCode = machine.MachineCode 
                    });
                }
                
                cboMachineCodeFilter.DisplayMember = "DisplayName";
                cboMachineCodeFilter.SelectedIndex = 0;
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载机器码列表失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载监控物品分类列表（从缓存）
        /// </summary>
        private async Task LoadMonitoredCategoriesAsync()
        {
            try
            {
                var cache = CacheService.Instance;
                var categories = cache.GetCategories();
                
                cboMonitoredCategoryFilter.Items.Clear();
                cboMonitoredCategoryFilter.Items.Add("所有分类");
                cboMonitoredCategoryFilter.Items.AddRange(categories.ToArray());
                cboMonitoredCategoryFilter.DisplayMember = "Name";
                cboMonitoredCategoryFilter.ValueMember = "Id";
                cboMonitoredCategoryFilter.SelectedIndex = 0;
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载监控物品分类失败: {ex.Message}");
            }
        }

        private async void BtnRefreshMonitoredItems_Click(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "正在刷新缓存...";
                btnRefreshMonitoredItems.Enabled = false;
                
                // 刷新分类、机器授权和监控物品缓存
                var cache = CacheService.Instance;
                await cache.RefreshCategoriesAsync();
                await cache.RefreshMachinesAsync();
                await cache.RefreshMonitoredItemsAsync();
                
                // 性能优化：刷新机器名称缓存
                await LoadMachineDisplayNameCacheAsync();
                
                // 重新加载筛选器和数据
                await LoadMachineCodesAsync();
                await LoadMonitoredCategoriesAsync();
                await LoadMonitoredItemsFilterAsync();
                await LoadMonitoredItemsAsync();
                
                lblStatus.Text = "刷新完成";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "刷新失败";
            }
            finally
            {
                btnRefreshMonitoredItems.Enabled = true;
            }
        }
        
        /// <summary>
        /// 搜索按钮点击事件
        /// </summary>
        private async void BtnSearchMonitoredItems_Click(object? sender, EventArgs e)
        {
            try
            {
                var searchText = txtSearchMonitoredItem.Text.Trim();
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // 如果搜索框为空，显示正常筛选的数据
                    await LoadMonitoredItemsAsync();
                    return;
                }
                
                lblStatus.Text = $"正在搜索: {searchText}";
                btnSearchMonitoredItems.Enabled = false;
                
                var cache = CacheService.Instance;
                var allItems = cache.GetMonitoredItems();
                
                // 在缓存中搜索（支持部分匹配）
                var searchResults = allItems
                    .Where(x => x.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.ItemName)
                    .ToList();
                
                dgvMonitoredItems.DataSource = searchResults;
                lblTotalItems.Text = $"总计: {searchResults.Count} 项";
                lblStatus.Text = $"搜索到 {searchResults.Count} 个匹配的监控物品";
                
                if (searchResults.Count == 0)
                {
                    MessageBox.Show($"未找到包含 \"{searchText}\" 的监控物品！", "搜索结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "搜索失败";
            }
            finally
            {
                btnSearchMonitoredItems.Enabled = true;
            }
        }

        private async Task LoadMonitoredItemsAsync()
        {
            try
            {
                lblStatus.Text = "正在加载监控物品...";
                btnRefreshMonitoredItems.Enabled = false;
                
                var cache = CacheService.Instance;
                var items = cache.GetMonitoredItems();
                
                // 机器码筛选（在内存中）
                if (cboMachineCodeFilter.SelectedIndex > 0 && cboMachineCodeFilter.SelectedItem is MachineCodeItem machineItem)
                {
                    items = items.Where(x => x.MachineCode == machineItem.MachineCode).ToList();
                }
                
                // 分类筛选（在内存中）
                if (cboMonitoredCategoryFilter.SelectedIndex > 0 && cboMonitoredCategoryFilter.SelectedItem is ItemCategory category)
                {
                    items = items.Where(x => x.CategoryId == category.Id).ToList();
                }
                
                // 物品名称筛选（在内存中）
                if (cboMonitoredItemFilter.SelectedIndex > 0)
                {
                    var itemName = cboMonitoredItemFilter.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(itemName) && itemName != "所有物品")
                    {
                        items = items.Where(x => x.ItemName == itemName).ToList();
                    }
                }
                
                // 性能优化：暂停布局更新
                dgvMonitoredItems.SuspendLayout();
                try
                {
                    dgvMonitoredItems.DataSource = null; // 先清空，减少中间状态的绘制
                    dgvMonitoredItems.DataSource = items;
                }
                finally
                {
                    dgvMonitoredItems.ResumeLayout();
                }
                
                lblTotalItems.Text = $"总计: {items.Count} 项";
                lblStatus.Text = $"已加载 {items.Count} 个监控物品";
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "加载失败";
            }
            finally
            {
                btnRefreshMonitoredItems.Enabled = true;
            }
        }
        
        /// <summary>
        /// 机器码筛选变化事件
        /// </summary>
        private async void CboMachineCodeFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await LoadMonitoredItemsAsync();
            await LoadMonitoredItemsFilterAsync();
        }
        
        /// <summary>
        /// 监控物品分类筛选变化事件
        /// </summary>
        private async void CboMonitoredCategoryFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await LoadMonitoredItemsAsync();
            await LoadMonitoredItemsFilterAsync();
        }
        
        /// <summary>
        /// 监控物品筛选变化事件
        /// </summary>
        private async void CboMonitoredItemFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await LoadMonitoredItemsAsync();
        }
        
        /// <summary>
        /// 加载监控物品名称筛选列表（从缓存）
        /// </summary>
        private async Task LoadMonitoredItemsFilterAsync()
        {
            try
            {
                var cache = CacheService.Instance;
                var items = cache.GetMonitoredItems();
                
                // 根据机器码和分类筛选（在内存中）
                if (cboMachineCodeFilter.SelectedIndex > 0 && cboMachineCodeFilter.SelectedItem is MachineCodeItem machineItem)
                {
                    items = items.Where(x => x.MachineCode == machineItem.MachineCode).ToList();
                }
                
                if (cboMonitoredCategoryFilter.SelectedIndex > 0 && cboMonitoredCategoryFilter.SelectedItem is ItemCategory category)
                {
                    items = items.Where(x => x.CategoryId == category.Id).ToList();
                }
                
                var itemNames = items
                    .Select(x => x.ItemName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                
                var currentSelection = cboMonitoredItemFilter.SelectedItem?.ToString();
                
                cboMonitoredItemFilter.Items.Clear();
                cboMonitoredItemFilter.Items.Add("所有物品");
                cboMonitoredItemFilter.Items.AddRange(itemNames.ToArray());
                
                // 尝试恢复之前的选择
                if (currentSelection != null && cboMonitoredItemFilter.Items.Contains(currentSelection))
                {
                    cboMonitoredItemFilter.SelectedItem = currentSelection;
                }
                else
                {
                    cboMonitoredItemFilter.SelectedIndex = 0;
                }
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载监控物品筛选列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加监控物品
        /// </summary>
        private async void BtnAddMonitoredItem_Click(object? sender, EventArgs e)
        {
            try
            {
                // 打开添加窗体（传入 null 表示添加模式）
                using var addForm = new MonitoredItemEditForm(null);
                
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("添加成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 刷新缓存
                    var cache = CacheService.Instance;
                    await cache.RefreshMonitoredItemsAsync();
                    
                    // 重新加载数据
                    await LoadMonitoredItemsFilterAsync();
                    await LoadMonitoredItemsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 编辑监控物品
        /// </summary>
        private async void BtnEditMonitoredItem_Click(object? sender, EventArgs e)
        {
            if (dgvMonitoredItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要编辑的监控物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                var item = (MonitoredItem)dgvMonitoredItems.SelectedRows[0].DataBoundItem;
                
                // 重新从数据库加载以确保有完整的导航属性
                using var context = new Aion2DbContext();
                var fullItem = await context.MonitoredItems
                    .Include(x => x.ItemCategory)
                    .FirstOrDefaultAsync(x => x.Id == item.Id);
                
                if (fullItem == null)
                {
                    MessageBox.Show("监控物品不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                using var editForm = new MonitoredItemEditForm(fullItem);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // 刷新缓存
                    var cache = CacheService.Instance;
                    await cache.RefreshMonitoredItemsAsync();
                    
                    // 重新加载数据
                    await LoadMonitoredItemsFilterAsync();
                    await LoadMonitoredItemsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编辑失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDeleteMonitoredItem_Click(object? sender, EventArgs e)
        {
            if (dgvMonitoredItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的监控物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var result = MessageBox.Show("确定要删除选中的监控物品吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    var item = (MonitoredItem)dgvMonitoredItems.SelectedRows[0].DataBoundItem;
                    using var context = new Aion2DbContext();
                    var service = new MonitoredItemService(context);
                    await service.DeleteMonitoredItemAsync(item.Id);
                    
                    MessageBox.Show("删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 刷新缓存
                    var cache = CacheService.Instance;
                    await cache.RefreshMonitoredItemsAsync();
                    
                    // 重新加载数据
                    await LoadMonitoredItemsFilterAsync();
                    await LoadMonitoredItemsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnRefreshPurchaseRecords_Click(object? sender, EventArgs e)
        {
            await LoadPurchaseRecordsAsync();
        }

        private async Task LoadPurchaseRecordsAsync()
        {
            try
            {
                lblStatus.Text = "正在加载购买记录...";
                btnRefreshPurchaseRecords.Enabled = false;
                
                using var context = new Aion2DbContext();
                
                var query = context.PurchaseRecords.AsQueryable();
                
                // 日期筛选
                var startDate = dtpStartDate.Value.Date;
                var endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(x => x.PurchaseTime >= startDate && x.PurchaseTime <= endDate);
                
                // 机器筛选
                if (cboMachineFilter.SelectedIndex > 0)
                {
                    var machineCode = cboMachineFilter.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(machineCode))
                    {
                        query = query.Where(x => x.MachineCode == machineCode);
                    }
                }
                
                var records = await query
                    .OrderByDescending(x => x.PurchaseTime)
                    .ToListAsync();
                
                dgvPurchaseRecords.DataSource = records;
                lblTotalRecords.Text = $"总计: {records.Count} 条";
                lblStatus.Text = $"已加载 {records.Count} 条购买记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "加载失败";
            }
            finally
            {
                btnRefreshPurchaseRecords.Enabled = true;
            }
        }

        private async void BtnRefreshStats_Click(object? sender, EventArgs e)
        {
            await LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                lblStatus.Text = "正在加载统计数据...";
                btnRefreshStats.Enabled = false;
                
                using var context = new Aion2DbContext();
                var dbService = new DatabaseService(context);
                
                // 获取活跃机器数
                var machines = await dbService.GetActiveMachineCodesAsync(30);
                lblTotalMachines.Text = machines.Count.ToString();
                
                // 获取总购买次数和利润
                var purchaseStats = await context.PurchaseRecords
                    .Where(x => x.Status == PurchaseStatus.Completed)
                    .Select(x => new
                    {
                        x.TotalAmount,
                        Profit = x.ActualProfit ?? x.ExpectedProfit
                    })
                    .ToListAsync();
                
                lblTotalPurchases.Text = purchaseStats.Count.ToString();
                lblTotalProfit.Text = $"{purchaseStats.Sum(x => x.Profit):N0}";
                
                // 加载机器列表到购买记录筛选器
                cboMachineFilter.Items.Clear();
                cboMachineFilter.Items.Add("所有机器");
                cboMachineFilter.Items.AddRange(machines.ToArray());
                cboMachineFilter.SelectedIndex = 0;
                
                lblStatus.Text = "统计数据加载完成";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载统计失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "加载失败";
            }
            finally
            {
                btnRefreshStats.Enabled = true;
            }
        }

        private void CreateItemsTab()
        {
            var tabPage = new TabPage("物品管理");
            
            // 工具栏
            var toolPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            
            txtSearchItemName = new TextBox { Location = new Point(15, 25), Width = 250, PlaceholderText = "搜索物品名称..." };
            txtSearchItemName.TextChanged += TxtSearchItemName_TextChanged;
            
            var lblCategory = new Label { Text = "分类:", Location = new Point(280, 30), AutoSize = true };
            cboCategoryFilter = new ComboBox { Location = new Point(330, 25), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategoryFilter.SelectedIndexChanged += CboCategoryFilter_SelectedIndexChanged;
            
            btnRefreshItems = new Button { Text = "🔄 刷新", Location = new Point(550, 23), Size = new Size(100, 30) };
            btnAddItem = new Button { Text = "➕ 添加", Location = new Point(665, 23), Size = new Size(100, 30), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White };
            btnEditItem = new Button { Text = "✏️ 编辑", Location = new Point(780, 23), Size = new Size(100, 30), BackColor = Color.FromArgb(0, 123, 255), ForeColor = Color.White };
            btnDeleteItem = new Button { Text = "🗑️ 删除", Location = new Point(895, 23), Size = new Size(100, 30), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White };
            lblTotalItemsCount = new Label { Text = "总计: 0 项", Location = new Point(1015, 30), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold) };
            
            btnRefreshItems.Click += BtnRefreshItems_Click;
            btnAddItem.Click += BtnAddItem_Click;
            btnEditItem.Click += BtnEditItem_Click;
            btnDeleteItem.Click += BtnDeleteItem_Click;
            
            toolPanel.Controls.AddRange(new Control[] { txtSearchItemName, lblCategory, cboCategoryFilter, btnRefreshItems, btnAddItem, btnEditItem, btnDeleteItem, lblTotalItemsCount });
            
            // 数据表格
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersWidth = 60,
                Font = new Font("Microsoft YaHei UI", 10F),
                RowTemplate = { Height = 32 },
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 240, 240),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(5, 0, 5, 0)
                }
            };
            
            // 性能优化：启用双缓冲
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvItems, new object[] { true });
            
            dgvItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "物品名称", DataPropertyName = "Name", Width = 250 },
                new DataGridViewTextBoxColumn { HeaderText = "分类", Name = "CategoryColumn", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "等级", DataPropertyName = "ItemLevel", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "品质", DataPropertyName = "Quality", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "参考价格", DataPropertyName = "ReferencePrice", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } },
                new DataGridViewCheckBoxColumn { HeaderText = "可交易", DataPropertyName = "IsTradable", Width = 100 },
                new DataGridViewCheckBoxColumn { HeaderText = "启用", DataPropertyName = "IsEnabled", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "备注", DataPropertyName = "Remarks", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreatedAt", Width = 200, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } }
            });
            
            // 处理分类列的显示
            dgvItems.CellFormatting += DgvItems_CellFormatting;
            
            tabPage.Controls.Add(dgvItems);
            tabPage.Controls.Add(toolPanel);
            tabControl.TabPages.Add(tabPage);
        }

        private void DgvItems_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvItems.Columns[e.ColumnIndex].Name == "CategoryColumn" && e.RowIndex >= 0)
            {
                var item = dgvItems.Rows[e.RowIndex].DataBoundItem as Item;
                if (item?.Category != null)
                {
                    e.Value = item.Category.Name;
                    e.FormattingApplied = true;
                }
            }
        }

        private async void BtnRefreshItems_Click(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "正在刷新缓存...";
                btnRefreshItems.Enabled = false;
                
                // 刷新物品和分类缓存
                var cache = CacheService.Instance;
                await cache.RefreshItemsAsync();
                await cache.RefreshCategoriesAsync();
                
                // 重新加载界面数据
                await LoadItemsAsync();
                await LoadItemCategoriesAsync();
                
                lblStatus.Text = "刷新完成";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "刷新失败";
            }
            finally
            {
                btnRefreshItems.Enabled = true;
            }
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                lblStatus.Text = "正在加载物品...";
                btnRefreshItems.Enabled = false;
                
                var cache = CacheService.Instance;
                var items = cache.GetItems();
                
                // 性能优化：暂停布局更新
                dgvItems.SuspendLayout();
                try
                {
                    dgvItems.DataSource = null; // 先清空，减少中间状态的绘制
                    dgvItems.DataSource = items;
                }
                finally
                {
                    dgvItems.ResumeLayout();
                }
                
                lblTotalItemsCount.Text = $"总计: {items.Count} 项";
                lblStatus.Text = $"已加载 {items.Count} 个物品";
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "加载失败";
            }
            finally
            {
                btnRefreshItems.Enabled = true;
            }
        }

        private async Task LoadItemCategoriesAsync()
        {
            try
            {
                var cache = CacheService.Instance;
                var categories = cache.GetCategories();
                
                cboCategoryFilter.Items.Clear();
                cboCategoryFilter.Items.Add("所有分类");
                cboCategoryFilter.Items.AddRange(categories.ToArray());
                cboCategoryFilter.DisplayMember = "Name";
                cboCategoryFilter.ValueMember = "Id";
                cboCategoryFilter.SelectedIndex = 0;
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载分类失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CboCategoryFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboCategoryFilter.SelectedIndex <= 0)
            {
                await LoadItemsAsync();
                return;
            }

            try
            {
                lblStatus.Text = "正在筛选物品...";
                
                var cache = CacheService.Instance;
                var category = cboCategoryFilter.SelectedItem as ItemCategory;
                if (category != null)
                {
                    var items = cache.GetItemsByCategory(category.Id);
                    dgvItems.DataSource = items;
                    lblTotalItemsCount.Text = $"总计: {items.Count} 项";
                    lblStatus.Text = $"已筛选 {items.Count} 个物品";
                }
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                MessageBox.Show($"筛选失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "筛选失败";
            }
        }

        private async void TxtSearchItemName_TextChanged(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchItemName.Text))
            {
                await LoadItemsAsync();
                return;
            }

            try
            {
                var cache = CacheService.Instance;
                var items = cache.SearchItems(txtSearchItemName.Text);
                dgvItems.DataSource = items;
                lblTotalItemsCount.Text = $"总计: {items.Count} 项";
                
                await Task.CompletedTask; // 保持异步签名
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAddItem_Click(object? sender, EventArgs e)
        {
            var form = new ItemEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                // 刷新缓存
                var cache = CacheService.Instance;
                await cache.RefreshItemsAsync();
                
                // 重新加载数据
                await LoadItemsAsync();
            }
        }

        private async void BtnEditItem_Click(object? sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要编辑的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var item = (Item)dgvItems.SelectedRows[0].DataBoundItem;
            
            // 重新从数据库加载以确保有完整的导航属性
            using var context = new Aion2DbContext();
            var service = new ItemService(context);
            var fullItem = await service.GetItemByIdAsync(item.Id);
            
            if (fullItem == null)
            {
                MessageBox.Show("物品不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var form = new ItemEditForm(fullItem);
            if (form.ShowDialog() == DialogResult.OK)
            {
                // 刷新缓存
                var cache = CacheService.Instance;
                await cache.RefreshItemsAsync();
                
                // 重新加载数据
                await LoadItemsAsync();
            }
        }

        private async void BtnDeleteItem_Click(object? sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var result = MessageBox.Show("确定要删除选中的物品吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    var item = (Item)dgvItems.SelectedRows[0].DataBoundItem;
                    using var context = new Aion2DbContext();
                    var service = new ItemService(context);
                    
                    if (await service.DeleteItemAsync(item.Id))
                    {
                        MessageBox.Show("删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // 刷新缓存
                        var cache = CacheService.Instance;
                        await cache.RefreshItemsAsync();
                        
                        // 重新加载数据
                        await LoadItemsAsync();
                    }
                    else
                    {
                        MessageBox.Show("删除失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        /// <summary>
        /// 创建AI智能管理标签页
        /// </summary>
        private void CreateAIManagementTab()
        {
            var tabPage = new TabPage("🤖 AI智能管理");
            
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), AutoScroll = true };
            
            // 标题
            var lblTitle = new Label 
            { 
                Text = "🤖 AI智能分析配置", 
                Location = new Point(30, 20), 
                AutoSize = true, 
                Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241)
            };
            
            // AI配置按钮
            var btnAIConfig = new Button
            {
                Text = "⚙️ AI配置",
                Location = new Point(30, 70),
                Size = new Size(200, 80),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold)
            };
            btnAIConfig.Click += BtnAIConfig_Click;
            
            // 分析日志按钮
            var btnAnalysisLogs = new Button
            {
                Text = "📊 分析日志",
                Location = new Point(250, 70),
                Size = new Size(200, 80),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold)
            };
            btnAnalysisLogs.Click += BtnAnalysisLogs_Click;
            
            // 训练数据按钮
            var btnTrainingData = new Button
            {
                Text = "📚 训练数据",
                Location = new Point(470, 70),
                Size = new Size(200, 80),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold)
            };
            btnTrainingData.Click += BtnTrainingData_Click;
            
            // 功能说明
            var lblInfo = new Label
            {
                Text = "💡 功能说明：\n" +
                       "• AI配置：设置AI分析模式、权重、安全限制等\n" +
                       "• 分析日志：查看AI分析历史记录和准确率\n" +
                       "• 训练数据：查看历史交易数据，用于AI学习优化",
                Location = new Point(30, 170),
                Size = new Size(900, 100),
                Font = new Font("Microsoft YaHei UI", 10),
                ForeColor = Color.Gray
            };
            
            panel.Controls.AddRange(new Control[] { lblTitle, btnAIConfig, btnAnalysisLogs, btnTrainingData, lblInfo });
            tabPage.Controls.Add(panel);
            tabControl.TabPages.Add(tabPage);
        }
        
        private void BtnAIConfig_Click(object? sender, EventArgs e)
        {
            try
            {
                using var configForm = new AIConfigurationForm();
                configForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开AI配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnAnalysisLogs_Click(object? sender, EventArgs e)
        {
            try
            {
                using var logsForm = new AIAnalysisLogsForm();
                logsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开AI分析日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnTrainingData_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("AI训练数据功能开发中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    
    /// <summary>
    /// 机器码下拉列表项
    /// </summary>
    public class MachineCodeItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string MachineCode { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return DisplayName;
        }
    }
}

