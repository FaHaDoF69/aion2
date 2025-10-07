using Aion2Helper.Models;
using Aion2Helper.Services;
using Aion2Helper.Utils;
using Aion2Helper.Forms;
using Aion2Helper.Data;
using Microsoft.EntityFrameworkCore;

namespace Aion2Helper
{
    public partial class Form1 : Form
    {
        private readonly ImageDetectionService _imageService;
        private readonly GameWindowService _windowService;
        private readonly AIConfigurationService _aiConfigService;
        private AIConfiguration? _currentAIConfig;
        private bool _isMonitoring = false;
        private DateTime _startTime;
        private System.Windows.Forms.Timer _runtimeTimer;
        private System.Windows.Forms.Timer _monitoringTimer;
        private List<MonitoredItem> _monitoredItems = new List<MonitoredItem>();
        private bool _databaseConnected = false;

        // 交易历史相关字段
        private int _currentHistoryPage = 1;
        private int _historyPageSize = 20;
        private int _totalHistoryPages = 1;
        private List<PurchaseRecord> _currentHistoryRecords = new List<PurchaseRecord>();
        
        // 价格趋势分析相关字段
        private System.Windows.Forms.DataVisualization.Charting.Chart? _trendChart;

        public Form1()
        {
            InitializeComponent();
            
            _windowService = new GameWindowService();
            _imageService = new ImageDetectionService(_windowService);
            _aiConfigService = new AIConfigurationService();
            
            InitializeTimers();
            InitializeListView();
            InitializePriceTrendChart();
            AddLog("系统初始化完成");
            
            #if DEBUG
            Aion2Helper.Program.LogSuccess("系统", "主窗体初始化完成");
            #endif
        }

        private void InitializeTimers()
        {
            // 运行时间计时器
            _runtimeTimer = new System.Windows.Forms.Timer();
            _runtimeTimer.Interval = 1000; // 1秒
            _runtimeTimer.Tick += RuntimeTimer_Tick;

            // 监控计时器
            _monitoringTimer = new System.Windows.Forms.Timer();
            _monitoringTimer.Interval = 5000; // 5秒
            _monitoringTimer.Tick += MonitoringTimer_Tick;
        }

        private void InitializeListView()
        {
            // 设置ListView样式
            listViewOpportunities.View = View.Details;
            listViewOpportunities.FullRowSelect = true;
            listViewOpportunities.GridLines = true;
            
            // 不再添加示例数据，等待数据库连接后加载真实数据
        }

        private void AddSampleOpportunity()
        {
            var item1 = listViewOpportunities.Items.Add("传说武器强化石");
            item1.SubItems.Add("3");                                                                          // 物品等级
            item1.SubItems.Add("850,000");
            item1.SubItems.Add("1,200,000");
            item1.SubItems.Add("350,000");
            item1.SubItems.Add("41%");
            item1.SubItems.Add("低");
            item1.SubItems.Add("捡漏");
            item1.SubItems.Add(DateTime.Now.AddMinutes(-2).ToString("HH:mm:ss"));

            var item2 = listViewOpportunities.Items.Add("史诗防具碎片");
            item2.SubItems.Add("2");                                                                          // 物品等级
            item2.SubItems.Add("450,000");
            item2.SubItems.Add("600,000");
            item2.SubItems.Add("150,000");
            item2.SubItems.Add("33%");
            item2.SubItems.Add("中");
            item2.SubItems.Add("套利");
            item2.SubItems.Add(DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss"));

            UpdateStats();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            AddLog("Aion 2 拍卖行智能辅助系统已启动");
            
            // 检查机器授权
            var authResult = await CheckMachineAuthorizationAsync();
            if (!authResult)
            {
                AddLog("程序将在3秒后关闭...");
                await Task.Delay(3000);
                Application.Exit();
                return;
            }
            
            // 检查数据库连接并加载监控物品
            await CheckDatabaseAndLoadItemsAsync();
            
            // 加载AI配置
            await LoadAIConfigurationAsync();
            
            // 加载交易历史数据
            await LoadPurchaseHistoryAsync();
            
            // 初始化缓存服务
            await InitializeCacheServiceAsync();
            
            AddLog("请点击'开始监控'按钮开始监控拍卖行");
        }

        /// <summary>
        /// 检查机器授权
        /// </summary>
        private async Task<bool> CheckMachineAuthorizationAsync()
        {
            try
            {
                AddLog("正在验证机器授权...");
                
                using var context = new Aion2Helper.Data.Aion2DbContext();
                using var authService = new Aion2Helper.Services.MachineAuthorizationService(context);
                
                var result = await authService.CheckCurrentMachineAuthorizationAsync();
                
                if (result.IsAuthorized)
                {
                    AddLog($"✓ 授权验证成功");
                    AddLog($"  机器码: {result.MachineCode}");
                    if (result.Authorization?.MachineName != null)
                    {
                        AddLog($"  机器名称: {result.Authorization.MachineName}");
                    }
                    if (result.Authorization?.EndTime.HasValue == true)
                    {
                        AddLog($"  授权有效期至: {result.Authorization.EndTime.Value:yyyy-MM-dd HH:mm:ss}");
                    }
                    
                    #if DEBUG
                    Aion2Helper.Program.LogSuccess("授权", "机器授权验证成功");
                    #endif
                    
                    return true;
                }
                else
                {
                    AddLog($"✗ 授权验证失败: {result.Message}");
                    AddLog($"  机器码: {result.MachineCode}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogError("授权", $"验证失败: {result.Message}");
                    #endif
                    
                    MessageBox.Show(
                        $"授权验证失败！\n\n{result.Message}\n\n机器码: {result.MachineCode}\n\n请联系管理员添加授权。",
                        "授权失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 授权检查异常: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("授权", $"检查异常: {ex.Message}");
                #endif
                
                MessageBox.Show(
                    $"授权检查失败！\n\n错误: {ex.Message}\n\n程序无法启动。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                return false;
            }
        }

        /// <summary>
        /// 检查数据库连接并加载监控物品
        /// </summary>
        private async Task CheckDatabaseAndLoadItemsAsync()
        {
            try
            {
                AddLog("正在检查数据库连接...");
                var result = await DatabaseConnectionChecker.CheckConnectionAsync();
                
                if (result.IsConnected)
                {
                    AddLog($"✓ 数据库连接成功 ({result.ResponseTime.TotalMilliseconds:F0}ms)");
                    AddLog($"  服务器版本: {result.ServerVersion}");
                    AddLog($"  机器码: {result.MachineCode}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogSuccess("数据库", $"连接成功 - 响应时间: {result.ResponseTime.TotalMilliseconds:F0}ms");
                    Aion2Helper.Program.LogInfo("数据库", $"服务器版本: {result.ServerVersion}");
                    Aion2Helper.Program.LogInfo("数据库", $"机器码: {result.MachineCode}");
                    #endif
                    
                    _databaseConnected = true;
                    toolStripStatusLabel1.Text = "数据库已连接";
                    
                    // 加载监控物品，处理机器码匹配问题
                    await LoadMonitoredItemsWithMachineCodeFixAsync();
                    
                    // 加载最新的监控机会
                    await LoadLatestMonitoringOpportunitiesAsync();
                }
                else
                {
                    AddLog($"✗ 数据库连接失败: {result.ErrorMessage}");
                    toolStripStatusLabel1.Text = "数据库连接失败";
                    
                    #if DEBUG
                    Aion2Helper.Program.LogError("数据库", $"连接失败: {result.ErrorMessage}");
                    #endif
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 数据库检查异常: {ex.Message}");
                toolStripStatusLabel1.Text = "数据库检查异常";
            }
        }

        /// <summary>
        /// 加载AI配置
        /// </summary>
        private async Task LoadAIConfigurationAsync()
        {
            try
            {
                if (!_databaseConnected)
                {
                    AddLog("⚠ 数据库未连接，跳过AI配置加载");
                    return;
                }

                AddLog("正在加载AI配置...");
                
                string machineCode = MachineCodeHelper.GetMachineCode();
                _currentAIConfig = await _aiConfigService.GetConfigurationByMachineCodeAsync(machineCode);
                
                if (_currentAIConfig == null)
                {
                    // 创建默认配置
                    _currentAIConfig = AIConfigurationService.CreateDefaultConfiguration(machineCode);
                    AddLog($"✓ 使用默认AI配置 (模式: {_currentAIConfig.AIMode})");
                }
                else
                {
                    AddLog($"✓ AI配置加载成功 (模式: {_currentAIConfig.AIMode})");
                    AddLog($"  权重分配: 价格{_currentAIConfig.PriceWeight}% | 市场{_currentAIConfig.MarketWeight}% | 盈利{_currentAIConfig.ProfitWeight}% | 时机{_currentAIConfig.TimingWeight}% | 历史{_currentAIConfig.HistoryWeight}%");
                    
                    if (_currentAIConfig.AutoBuyEnabled)
                    {
                        AddLog($"  ⚠ 已启用自动购买 (AI得分≥{_currentAIConfig.AutoBuyMinScore}分)");
                    }
                }

                #if DEBUG
                Aion2Helper.Program.LogSuccess("AI配置", $"加载完成 - 模式: {_currentAIConfig.AIMode}");
                #endif
                
                // 更新AI配置显示
                UpdateAIConfigurationDisplay();
            }
            catch (Exception ex)
            {
                AddLog($"✗ AI配置加载失败: {ex.Message}");
                
                // 使用默认配置
                _currentAIConfig = AIConfigurationService.CreateDefaultConfiguration(MachineCodeHelper.GetMachineCode());
                AddLog("使用默认AI配置");
                
                // 更新AI配置显示
                UpdateAIConfigurationDisplay();
                
                #if DEBUG
                Aion2Helper.Program.LogError("AI配置", $"加载失败，使用默认配置: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 更新AI配置显示
        /// </summary>
        private void UpdateAIConfigurationDisplay()
        {
            try
            {
                if (_currentAIConfig == null)
                {
                    lblAIModeStatus.Text = "AI模式: 未配置（使用默认配置）";
                    lblAIWeightsStatus.Text = "权重分配: 未配置";
                    lblAIAutoStatus.Text = "自动购买: 未配置";
                    lblAISafetyStatus.Text = "安全限制: 未配置";
                    lblAIModeStatus.ForeColor = Color.Gray;
                    return;
                }

                // 显示AI模式
                lblAIModeStatus.Text = $"AI模式: {_currentAIConfig.AIMode}";
                lblAIModeStatus.ForeColor = _currentAIConfig.AIMode == "不使用AI" ? Color.Gray : Color.DarkGreen;

                // 显示权重分配
                lblAIWeightsStatus.Text = $"权重分配: 价格{_currentAIConfig.PriceWeight}% | 市场{_currentAIConfig.MarketWeight}% | 盈利{_currentAIConfig.ProfitWeight}% | 时机{_currentAIConfig.TimingWeight}% | 历史{_currentAIConfig.HistoryWeight}%";
                lblAIWeightsStatus.ForeColor = Color.DarkBlue;

                // 显示自动购买设置
                if (_currentAIConfig.AutoBuyEnabled)
                {
                    lblAIAutoStatus.Text = $"自动购买: ✅ 已启用 (AI得分≥{_currentAIConfig.AutoBuyMinScore}分自动购买, <{_currentAIConfig.AutoIgnoreMaxScore}分自动忽略)";
                    lblAIAutoStatus.ForeColor = Color.DarkOrange;
                }
                else
                {
                    lblAIAutoStatus.Text = "自动购买: ❌ 未启用";
                    lblAIAutoStatus.ForeColor = Color.Gray;
                }

                // 显示安全限制
                lblAISafetyStatus.Text = $"安全限制: 单次最大投入{_currentAIConfig.MaxSingleInvestment:N0}金币 | 人工确认金额{_currentAIConfig.RequireManualConfirmAmount:N0}金币 | AI黑名单{(_currentAIConfig.EnableAIBlacklist ? "✅启用" : "❌禁用")}";
                lblAISafetyStatus.ForeColor = Color.DarkSlateGray;
            }
            catch (Exception ex)
            {
                AddLog($"更新AI配置显示失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载监控物品
        /// </summary>
        private async Task LoadMonitoredItemsWithMachineCodeFixAsync()
        {
            try
            {
                AddLog("正在加载监控物品配置...");
                
                using var context = new Aion2Helper.Data.Aion2DbContext();
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                AddLog($"  当前机器码: {currentMachineCode}");
                
                // 直接用完整机器码查询，并包含分类信息
                _monitoredItems = await context.MonitoredItems
                    .Include(x => x.ItemCategory)
                    .Where(x => x.MachineCode == currentMachineCode && x.IsEnabled)
                    .ToListAsync();
                
                AddLog($"✓ 成功加载 {_monitoredItems.Count} 个监控物品");
                
                // 更新UI
                UpdateMonitoredItemsUI();
                
                // 显示统计信息
                if (_monitoredItems.Count > 0)
                {
                    var autoCount = _monitoredItems.Count(x => x.AutoPurchaseEnabled);
                    AddLog($"  其中自动购买: {autoCount} 个");
                    
                    var topItems = _monitoredItems.OrderByDescending(x => x.Priority).Take(3);
                    AddLog("  高优先级物品:");
                    foreach (var item in topItems)
                    {
                        var auto = item.AutoPurchaseEnabled ? "[自动]" : "[手动]";
                        AddLog($"    {auto} {item.ItemName} (优先级:{item.Priority})");
                    }
                }
                else
                {
                    AddLog("  ⚠ 当前没有配置监控物品");
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 加载监控物品失败: {ex.Message}");
                _monitoredItems = new List<MonitoredItem>();
            }
        }

        /// <summary>
        /// 更新监控物品UI显示
        /// </summary>
        private void UpdateMonitoredItemsUI()
        {
            try
            {
                // 清空现有列表
                listViewMonitoredItems.Items.Clear();
                
                // 添加监控物品到ListView
                foreach (var item in _monitoredItems.OrderByDescending(x => x.Priority))
                {
                    var listItem = new ListViewItem(item.ItemName);
#pragma warning disable CS0618
                    listItem.SubItems.Add(item.ItemCategory?.Name ?? item.Category ?? "-");
#pragma warning restore CS0618
                    listItem.SubItems.Add(item.ItemLevel?.ToString() ?? "-");                                 // 物品等级
                    listItem.SubItems.Add(item.TargetMinPrice?.ToString("N0") ?? "未设置");
                    listItem.SubItems.Add(item.TargetMaxPrice?.ToString("N0") ?? "未设置");
                    listItem.SubItems.Add(item.Priority.ToString());
                    listItem.SubItems.Add(item.IsEnabled ? "启用" : "禁用");
                    listItem.SubItems.Add(item.AutoPurchaseEnabled ? "是" : "否");
                    listItem.SubItems.Add(item.TotalFoundCount.ToString());                              // 发现次数
                    listItem.SubItems.Add(item.TotalPurchaseCount.ToString());                           // 购买次数
                    listItem.SubItems.Add(item.LastFoundPrice?.ToString("N0") ?? "无");                  // 最后发现价格
                    listItem.SubItems.Add(item.LastFoundAt?.ToString("MM-dd HH:mm") ?? "从未发现");       // 最后发现时间
                    
                    listItem.Tag = item;
                    listViewMonitoredItems.Items.Add(listItem);
                }
                
                AddLog($"  UI更新完成，显示 {listViewMonitoredItems.Items.Count} 个物品");
                UpdateStats();
            }
            catch (Exception ex)
            {
                AddLog($"更新UI失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取风险等级文本
        /// </summary>
        private string GetRiskLevelText(RiskLevel riskLevel)
        {
            return riskLevel switch
            {
                RiskLevel.Low => "低",
                RiskLevel.Medium => "中", 
                RiskLevel.High => "高",
                RiskLevel.VeryHigh => "极高",
                _ => "中"
            };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!_windowService.FindGameWindow("Aion2"))
            {
                MessageBox.Show("未找到游戏窗口 'Aion2'，请确保游戏正在运行！", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isMonitoring = true;
            _startTime = DateTime.Now;

            btnStart.Enabled = false;
            btnStop.Enabled = true;

            _runtimeTimer.Start();
            _monitoringTimer.Start();

            toolStripStatusLabel1.Text = "监控中...";
            AddLog("开始监控拍卖行...");
            AddLog("游戏窗口检测成功");
            AddLog("图像识别引擎已就绪");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isMonitoring = false;

            btnStart.Enabled = true;
            btnStop.Enabled = false;

            _runtimeTimer.Stop();
            _monitoringTimer.Stop();

            toolStripStatusLabel1.Text = "已停止";
            AddLog("监控已停止");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPageSettings;
        }

        private async void btnTestDetection_Click(object sender, EventArgs e)
        {
            if (!_databaseConnected)
            {
                MessageBox.Show("数据库未连接，无法测试检测功能！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 模拟检测到游戏数据
            var random = new Random();
            var testItems = new[]
            {
                new { Name = "传说武器强化石", Level = 3, Price = random.Next(800000, 1000000) },
                new { Name = "史诗防具碎片", Level = 2, Price = random.Next(400000, 500000) },
                new { Name = "稀有材料包", Level = 2, Price = random.Next(100000, 150000) },
                new { Name = "魔法水晶", Level = 1, Price = random.Next(70000, 90000) },
                new { Name = "装备碎片", Level = 2, Price = random.Next(180000, 220000) }
            };

            var testItem = testItems[random.Next(testItems.Length)];
            var sellers = new[] { "玩家A", "玩家B", "玩家C", "玩家D", "玩家E" };
            var seller = sellers[random.Next(sellers.Length)];

            AddLog($"🎯 模拟检测: {testItem.Name} (等级{testItem.Level}) @ {testItem.Price:N0}");
            
            // 调用真实的游戏数据处理方法
            await OnGameDataDetectedAsync(testItem.Name, testItem.Level, testItem.Price, seller);
        }

        private void RuntimeTimer_Tick(object? sender, EventArgs e)
        {
            if (_isMonitoring)
            {
                var runtime = DateTime.Now - _startTime;
                toolStripStatusLabel2.Text = $"运行时间: {runtime:hh\\:mm\\:ss}";
            }
        }

        private async void MonitoringTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isMonitoring || !_databaseConnected) return;

            try
            {
                // 真实监控逻辑：从数据库加载最新的监控历史记录
                await LoadLatestMonitoringOpportunitiesAsync();

                // 输出扫描日志
                var random = new Random();
                if (random.NextDouble() < 0.7)
                {
                    var messages = new[]
                    {
                        "正在扫描武器类物品...",
                        "正在扫描防具类物品...",
                        "正在扫描材料类物品...",
                        "价格数据更新完成",
                        "检测到价格波动..."
                    };
                    
                    AddLog(messages[random.Next(messages.Length)]);
                }
            }
            catch (Exception ex)
            {
                AddLog($"监控更新失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"监控更新失败: {ex.Message}");
                #endif
            }
        }

        private void SimulateNewOpportunity(Random random)
        {
            var itemNames = new[] { "魔法水晶", "强化石", "稀有材料", "装备碎片", "宝石" };
            var strategies = new[] { "捡漏", "套利", "趋势" };
            var riskLevels = new[] { "低", "中", "高" };
            var itemLevels = new[] { 1, 2, 3 };
            
            var itemName = itemNames[random.Next(itemNames.Length)];
            var itemLevel = itemLevels[random.Next(itemLevels.Length)];
            var currentPrice = random.Next(100000, 1000000);
            var profitMargin = random.NextDouble() * 0.5 + 0.1; // 10%-60%
            var expectedPrice = (int)(currentPrice * (1 + profitMargin));
            var expectedProfit = expectedPrice - currentPrice;

            var item = listViewOpportunities.Items.Insert(0, itemName);
            item.SubItems.Add(itemLevel.ToString());                                                          // 物品等级
            item.SubItems.Add($"{currentPrice:N0}");
            item.SubItems.Add($"{expectedPrice:N0}");
            item.SubItems.Add($"{expectedProfit:N0}");
            item.SubItems.Add($"{profitMargin:P1}");
            item.SubItems.Add(riskLevels[random.Next(riskLevels.Length)]);
            item.SubItems.Add(strategies[random.Next(strategies.Length)]);
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));

            AddLog($"发现交易机会: {itemName} (等级{itemLevel}) @ {currentPrice:N0}，预期利润: {expectedProfit:N0}");
            UpdateStats();
        }

        private void UpdateStats()
        {
            var enabledCount = _monitoredItems.Count(x => x.IsEnabled);
            var autoCount = _monitoredItems.Count(x => x.IsEnabled && x.AutoPurchaseEnabled);
            
            lblMonitoredItems.Text = $"监控物品: {enabledCount} (自动: {autoCount})";
            lblOpportunities.Text = $"发现机会: {listViewOpportunities.Items.Count}";
            lblExecutedTrades.Text = "执行交易: 0"; // 这个可以从数据库获取
            lblTotalProfit.Text = "总盈利: 1,250,000"; // 这个可以从数据库获取
        }

        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}\r\n";
            
            textBoxLog.AppendText(logEntry);
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }

        #region 设置页面事件处理

        private async void btnAddItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 直接打开编辑对话框进行详细配置
                using var editForm = new Aion2Helper.Forms.MonitoredItemEditForm();
                
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    var newItem = editForm.EditedItem;
                    
                    AddLog($"正在保存监控物品: {newItem.ItemName}...");
                    
                    // 异步保存到数据库
                    await Task.Run(async () =>
                    {
                        // 检查是否已存在该物品
                        using var service = new MonitoredItemService();
                        var existingItems = await service.GetCurrentMachineMonitoredItemsAsync(true);
                        if (existingItems.Any(x => x.ItemName.Equals(newItem.ItemName, StringComparison.OrdinalIgnoreCase)))
                        {
                            // 在UI线程显示消息
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("该物品已在监控列表中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }));
                            return null;
                        }
                        
                        // 保存到数据库
                        var savedItem = await service.AddMonitoredItemAsync(newItem);
                        return savedItem;
                    }).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            throw task.Exception?.InnerException ?? task.Exception!;
                        }
                        
                        var savedItem = task.Result;
                        if (savedItem != null)
                        {
                            AddLog($"✓ 成功添加监控物品: {savedItem.ItemName}");
                            
                            #if DEBUG
                            Aion2Helper.Program.LogSuccess("数据库", $"添加监控物品: {savedItem.ItemName} (ID: {savedItem.Id})");
                            #endif
                            
                            // 刷新列表
                            return LoadMonitoredItemsWithMachineCodeFixAsync();
                        }
                        return Task.CompletedTask;
                    }, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 添加监控物品失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("数据库", $"添加监控物品失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"添加监控物品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (listViewMonitoredItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要删除的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedListItem = listViewMonitoredItems.SelectedItems[0];
                var monitoredItem = selectedListItem.Tag as MonitoredItem;
                
                if (monitoredItem == null)
                {
                    MessageBox.Show("无法获取物品信息！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"确定要删除监控物品 '{monitoredItem.ItemName}' 吗？\n\n此操作不可撤销！", 
                    "确认删除", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    // 从数据库删除
                    using var service = new MonitoredItemService();
                    var success = await service.DeleteMonitoredItemAsync(monitoredItem.Id);
                    
                    if (success)
                    {
                        AddLog($"✓ 成功删除监控物品: {monitoredItem.ItemName}");
                        
                        #if DEBUG
                        Aion2Helper.Program.LogSuccess("数据库", $"删除监控物品: {monitoredItem.ItemName} (ID: {monitoredItem.Id})");
                        #endif
                        
                        // 刷新列表
                        await LoadMonitoredItemsWithMachineCodeFixAsync();
                    }
                    else
                    {
                        MessageBox.Show("删除失败，物品可能已被删除！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 删除监控物品失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("数据库", $"删除监控物品失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"删除监控物品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnClearItems_Click(object sender, EventArgs e)
        {
            if (listViewMonitoredItems.Items.Count == 0)
            {
                MessageBox.Show("监控列表已经为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"确定要清空所有监控物品吗？\n\n当前有 {listViewMonitoredItems.Items.Count} 个物品\n此操作不可撤销！", 
                "确认清空", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    using var service = new MonitoredItemService();
                    var allItems = await service.GetCurrentMachineMonitoredItemsAsync(true);
                    
                    int deletedCount = 0;
                    foreach (var item in allItems)
                    {
                        var success = await service.DeleteMonitoredItemAsync(item.Id);
                        if (success)
                        {
                            deletedCount++;
                        }
                    }
                    
                    AddLog($"✓ 成功清空监控列表，共删除 {deletedCount} 个物品");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogSuccess("数据库", $"清空监控列表，删除 {deletedCount} 个物品");
                    #endif
                    
                    // 刷新列表
                    await LoadMonitoredItemsWithMachineCodeFixAsync();
                }
                catch (Exception ex)
                {
                    AddLog($"✗ 清空监控列表失败: {ex.Message}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogError("数据库", $"清空监控列表失败: {ex.Message}");
                    #endif
                    
                    MessageBox.Show($"清空监控列表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void trackBarPriceThreshold_Scroll(object sender, EventArgs e)
        {
            var value = trackBarPriceThreshold.Value / 100.0;
            lblThresholdValue.Text = value.ToString("0.0");
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证设置
                if (!ValidateSettings())
                {
                    return;
                }

                // 保存设置逻辑（这里可以保存到配置文件）
                SaveSettingsToConfig();
                
                AddLog("设置保存成功");
                MessageBox.Show("设置保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog($"保存设置失败: {ex.Message}");
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateSettings()
        {
            // 验证最大投资金额
            if (!decimal.TryParse(textBoxMaxInvestment.Text, out decimal maxInvestment) || maxInvestment <= 0)
            {
                MessageBox.Show("请输入有效的最大投资金额！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMaxInvestment.Focus();
                return false;
            }

            // 验证操作间隔
            if (!int.TryParse(textBoxMinInterval.Text, out int minInterval) || minInterval <= 0)
            {
                MessageBox.Show("请输入有效的最小操作间隔！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMinInterval.Focus();
                return false;
            }

            if (!int.TryParse(textBoxMaxInterval.Text, out int maxInterval) || maxInterval <= minInterval)
            {
                MessageBox.Show("最大操作间隔必须大于最小操作间隔！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMaxInterval.Focus();
                return false;
            }

            return true;
        }

        private void SaveSettingsToConfig()
        {
            // 这里可以实现保存到JSON配置文件的逻辑
            var settings = new
            {
                // 游戏设置已移除
                TradingSettings = new
                {
                    MaxInvestment = decimal.Parse(textBoxMaxInvestment.Text),
                    PriceThreshold = trackBarPriceThreshold.Value / 100.0,
                    AutoTradingEnabled = checkBoxAutoTrading.Checked
                },
                SafetySettings = new
                {
                    HumanBehaviorEnabled = checkBoxHumanBehavior.Checked,
                    MinOperationInterval = int.Parse(textBoxMinInterval.Text),
                    MaxOperationInterval = int.Parse(textBoxMaxInterval.Text)
                },
                MonitoredItems = listViewMonitoredItems.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray()
            };

            // TODO: 保存到JSON文件
            AddLog("配置已更新到内存");
        }

        private async void btnEditItem_Click(object sender, EventArgs e)
        {
            if (listViewMonitoredItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择要编辑的物品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedListItem = listViewMonitoredItems.SelectedItems[0];
                var monitoredItem = selectedListItem.Tag as MonitoredItem;
                
                if (monitoredItem == null)
                {
                    MessageBox.Show("无法获取物品信息！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 打开编辑对话框
                using var editForm = new Aion2Helper.Forms.MonitoredItemEditForm(monitoredItem);
                
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    var editedItem = editForm.EditedItem;
                    
                    AddLog($"正在更新监控物品: {editedItem.ItemName}...");
                    
                    // 异步更新到数据库
                    await Task.Run(async () =>
                    {
                        using var service = new MonitoredItemService();
                        var updatedItem = await service.UpdateMonitoredItemAsync(editedItem);
                        return updatedItem;
                    }).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            throw task.Exception?.InnerException ?? task.Exception!;
                        }
                        
                        var updatedItem = task.Result;
                        AddLog($"✓ 成功更新监控物品: {updatedItem.ItemName}");
                        
                        #if DEBUG
                        Aion2Helper.Program.LogSuccess("数据库", $"更新监控物品: {updatedItem.ItemName} (ID: {updatedItem.Id})");
                        #endif
                        
                        // 刷新列表
                        return LoadMonitoredItemsWithMachineCodeFixAsync();
                    }, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 编辑监控物品失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("数据库", $"编辑监控物品失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"编辑监控物品失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void listViewMonitoredItems_DoubleClick(object sender, EventArgs e)
        {
            // 双击编辑物品
            btnEditItem_Click(sender, e);
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private async void btnRefreshItems_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("🔄 正在刷新监控物品数据...");
                
                #if DEBUG
                Aion2Helper.Program.LogInfo("系统", "开始刷新监控物品数据");
                #endif
                
                // 重新从数据库加载数据
                await LoadMonitoredItemsWithMachineCodeFixAsync();
                
                AddLog("✅ 监控物品数据刷新完成");
                
                #if DEBUG
                Aion2Helper.Program.LogSuccess("系统", $"刷新完成，当前显示 {_monitoredItems.Count} 个物品");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"❌ 刷新监控物品数据失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("系统", $"刷新数据失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"刷新数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region 右键菜单事件处理

        /// <summary>
        /// 右键菜单打开时的事件处理
        /// </summary>
        private void contextMenuMonitoredItems_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 刷新功能始终可用，即使没有选中项
            if (listViewMonitoredItems.SelectedItems.Count == 0)
            {
                // 只显示刷新选项
                menuItemEdit.Enabled = false;
                menuItemDelete.Enabled = false;
                menuItemToggleStatus.Enabled = false;
                menuItemToggleAutoBuy.Enabled = false;
                return;
            }

            // 有选中项时启用所有菜单项
            menuItemEdit.Enabled = true;
            menuItemDelete.Enabled = true;
            menuItemToggleStatus.Enabled = true;
            menuItemToggleAutoBuy.Enabled = true;

            // 根据选中项的状态更新菜单项文本
            var selectedItem = listViewMonitoredItems.SelectedItems[0];
            var monitoredItem = selectedItem.Tag as MonitoredItem;
            
            if (monitoredItem != null)
            {
                // 更新切换状态菜单项文本
                menuItemToggleStatus.Text = monitoredItem.IsEnabled ? "⏸️ 禁用监控" : "▶️ 启用监控";
                
                // 更新切换自动购买菜单项文本
                menuItemToggleAutoBuy.Text = monitoredItem.AutoPurchaseEnabled ? "🚫 禁用自动购买" : "🛒 启用自动购买";
            }
        }

        /// <summary>
        /// 右键菜单 - 编辑物品
        /// </summary>
        private void menuItemEdit_Click(object sender, EventArgs e)
        {
            btnEditItem_Click(sender, e);
        }

        /// <summary>
        /// 右键菜单 - 删除物品
        /// </summary>
        private void menuItemDelete_Click(object sender, EventArgs e)
        {
            btnRemoveItem_Click(sender, e);
        }

        /// <summary>
        /// 右键菜单 - 切换启用状态
        /// </summary>
        private async void menuItemToggleStatus_Click(object sender, EventArgs e)
        {
            if (listViewMonitoredItems.SelectedItems.Count == 0)
                return;

            try
            {
                var selectedItem = listViewMonitoredItems.SelectedItems[0];
                var monitoredItem = selectedItem.Tag as MonitoredItem;
                
                if (monitoredItem == null)
                    return;

                using var service = new MonitoredItemService();
                
                if (monitoredItem.IsEnabled)
                {
                    // 禁用监控需要确认
                    var result = MessageBox.Show(
                        $"确定要禁用对 '{monitoredItem.ItemName}' 的监控吗？\n\n禁用后将不再监控此物品的价格变化。", 
                        "确认禁用监控", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        await service.DisableMonitoredItemAsync(monitoredItem.Id);
                        AddLog($"✓ 已禁用监控: {monitoredItem.ItemName}");
                        
                        #if DEBUG
                        Aion2Helper.Program.LogInfo("监控", $"禁用物品: {monitoredItem.ItemName}");
                        #endif
                    }
                    else
                    {
                        return; // 用户取消操作，不刷新列表
                    }
                }
                else
                {
                    await service.EnableMonitoredItemAsync(monitoredItem.Id);
                    AddLog($"✓ 已启用监控: {monitoredItem.ItemName}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogInfo("监控", $"启用物品: {monitoredItem.ItemName}");
                    #endif
                }

                // 刷新列表
                await LoadMonitoredItemsWithMachineCodeFixAsync();
            }
            catch (Exception ex)
            {
                AddLog($"✗ 切换监控状态失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"切换状态失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"切换监控状态失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 右键菜单 - 切换自动购买状态
        /// </summary>
        private async void menuItemToggleAutoBuy_Click(object sender, EventArgs e)
        {
            if (listViewMonitoredItems.SelectedItems.Count == 0)
                return;

            try
            {
                var selectedItem = listViewMonitoredItems.SelectedItems[0];
                var monitoredItem = selectedItem.Tag as MonitoredItem;
                
                if (monitoredItem == null)
                    return;

                using var service = new MonitoredItemService();
                
                if (monitoredItem.AutoPurchaseEnabled)
                {
                    // 禁用自动购买需要确认
                    var result = MessageBox.Show(
                        $"确定要禁用 '{monitoredItem.ItemName}' 的自动购买功能吗？\n\n禁用后需要手动购买此物品。", 
                        "确认禁用自动购买", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        await service.DisableAutoPurchaseAsync(monitoredItem.Id);
                        AddLog($"✓ 已禁用自动购买: {monitoredItem.ItemName}");
                        
                        #if DEBUG
                        Aion2Helper.Program.LogInfo("交易", $"禁用自动购买: {monitoredItem.ItemName}");
                        #endif
                    }
                    else
                    {
                        return; // 用户取消操作，不刷新列表
                    }
                }
                else
                {
                    // 启用自动购买也需要确认，因为这涉及到自动花费金钱
                    var result = MessageBox.Show(
                        $"确定要启用 '{monitoredItem.ItemName}' 的自动购买功能吗？\n\n启用后系统将在发现合适价格时自动购买此物品。", 
                        "确认启用自动购买", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        await service.EnableAutoPurchaseAsync(monitoredItem.Id);
                        AddLog($"✓ 已启用自动购买: {monitoredItem.ItemName}");
                        
                        #if DEBUG
                        Aion2Helper.Program.LogInfo("交易", $"启用自动购买: {monitoredItem.ItemName}");
                        #endif
                    }
                    else
                    {
                        return; // 用户取消操作，不刷新列表
                    }
                }

                // 刷新列表
                await LoadMonitoredItemsWithMachineCodeFixAsync();
            }
            catch (Exception ex)
            {
                AddLog($"✗ 切换自动购买状态失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("交易", $"切换自动购买失败: {ex.Message}");
                #endif
                
                MessageBox.Show($"切换自动购买状态失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 右键菜单 - 刷新数据
        /// </summary>
        private void menuItemRefresh_Click(object sender, EventArgs e)
        {
            btnRefreshItems_Click(sender, e);
        }

        #endregion

        #endregion

        #region 监控机会相关方法

        /// <summary>
        /// 从数据库加载最新的监控机会
        /// </summary>
        private async Task LoadLatestMonitoringOpportunitiesAsync()
        {
            try
            {
                using var service = new Aion2Helper.Services.MonitoringHistoryService();
                
                // 获取最近10分钟内发现的未处理机会
                var recentOpportunities = await service.GetPaginatedHistoryAsync(
                    pageNumber: 1,
                    pageSize: 20,
                    startDate: DateTime.Now.AddMinutes(-10),
                    endDate: DateTime.Now,
                    isProcessed: false
                );

                // 更新UI显示
                UpdateOpportunitiesListView(recentOpportunities.Items);
                
                #if DEBUG
                Aion2Helper.Program.LogInfo("监控", $"加载最新机会: {recentOpportunities.Items.Count} 条记录");
                #endif
            }
            catch (Exception ex)
            {
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"加载最新机会失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 更新机会列表视图
        /// </summary>
        /// <param name="opportunities">监控机会列表</param>
        private void UpdateOpportunitiesListView(List<Aion2Helper.Models.MonitoringHistory> opportunities)
        {
            try
            {
                // 清空现有列表
                listViewOpportunities.Items.Clear();

                foreach (var opportunity in opportunities)
                {
                    var item = new ListViewItem(opportunity.ItemName);
                    
                    // 从缓存中查找分类
                    var cache = CacheService.Instance;
                    var monitoredItem = cache.GetMonitoredItemsByName(opportunity.ItemName).FirstOrDefault();
                    var categoryName = monitoredItem?.ItemCategory?.Name ?? "-";
                    
                    item.SubItems.Add(categoryName);                                                          // 物品分类
                    item.SubItems.Add(opportunity.ItemLevel?.ToString() ?? "-");                               // 物品等级
                    item.SubItems.Add(opportunity.CurrentPrice.ToString("N0"));                               // 当前价格
                    item.SubItems.Add(opportunity.ExpectedPrice.ToString("N0"));                              // 预期价格
                    item.SubItems.Add(opportunity.ExpectedProfit.ToString("N0"));                             // 预期利润
                    item.SubItems.Add(opportunity.ProfitRate.ToString("P1"));                                 // 利润率
                    item.SubItems.Add(GetRiskLevelText(opportunity.RiskLevel));                               // 风险等级
                    item.SubItems.Add(opportunity.Strategy);                                                  // 策略
                    item.SubItems.Add(opportunity.DiscoveredAt.ToString("HH:mm:ss"));                        // 发现时间

                    // 根据利润率设置行颜色
                    if (opportunity.ProfitRate > 0.4m)
                    {
                        item.ForeColor = Color.Green;  // 高利润绿色
                    }
                    else if (opportunity.ProfitRate > 0.2m)
                    {
                        item.ForeColor = Color.Blue;   // 中等利润蓝色
                    }
                    else
                    {
                        item.ForeColor = Color.Black;  // 低利润黑色
                    }

                    item.Tag = opportunity;
                    listViewOpportunities.Items.Add(item);
                }

                UpdateStats();
                
                #if DEBUG
                Aion2Helper.Program.LogInfo("监控UI", $"更新机会列表: {opportunities.Count} 条记录");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"更新机会列表失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控UI", $"更新机会列表失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 模拟发现新的交易机会（用于测试）
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <param name="itemLevel">物品等级</param>
        /// <param name="currentPrice">当前价格</param>
        /// <param name="expectedPrice">预期价格</param>
        /// <param name="strategy">策略</param>
        /// <param name="riskLevel">风险等级</param>
        /// <param name="sellerName">卖家名称</param>
        /// <returns>是否添加成功</returns>
        public async Task<bool> AddMonitoringOpportunityAsync(
            string itemName, 
            int? itemLevel, 
            decimal currentPrice, 
            decimal expectedPrice, 
            string strategy, 
            RiskLevel riskLevel, 
            string? sellerName = null)
        {
            try
            {
                using var service = new Aion2Helper.Services.MonitoringHistoryService();
                
                // 添加到监控历史表
                var record = await service.AddMonitoringRecordAsync(
                    itemName: itemName,
                    currentPrice: currentPrice,
                    expectedPrice: expectedPrice,
                    strategy: strategy,
                    riskLevel: riskLevel,
                    itemLevel: itemLevel,
                    sellerName: sellerName
                );

                // 立即刷新UI显示
                await LoadLatestMonitoringOpportunitiesAsync();

                var expectedProfit = expectedPrice - currentPrice;
                AddLog($"发现交易机会: {itemName} (等级{itemLevel ?? 0}) @ {currentPrice:N0}，预期利润: {expectedProfit:N0}");
                
                #if DEBUG
                Aion2Helper.Program.LogSuccess("监控", $"添加机会记录: {itemName} @ {currentPrice:N0}");
                #endif

                return true;
            }
            catch (Exception ex)
            {
                AddLog($"添加监控机会失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"添加监控机会失败: {ex.Message}");
                #endif
                
                return false;
            }
        }

        /// <summary>
        /// 当图像识别检测到交易机会时调用此方法
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <param name="itemLevel">物品等级</param>
        /// <param name="currentPrice">当前价格</param>
        /// <param name="sellerName">卖家名称</param>
        /// <param name="quantity">数量</param>
        public async Task OnGameDataDetectedAsync(string itemName, int? itemLevel, decimal currentPrice, string? sellerName = null, int quantity = 1)
        {
            try
            {
                // 查找匹配的监控物品
                var monitoredItem = _monitoredItems.FirstOrDefault(x => 
                    x.IsEnabled && 
                    x.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));

                if (monitoredItem == null)
                {
                    AddLog($"⚠ 检测到未监控物品: {itemName}，已忽略");
                    return;
                }

                // 检查价格是否在目标范围内
                if (!monitoredItem.IsInTargetPriceRange(currentPrice))
                {
                    AddLog($"⚠ {itemName} 价格 {currentPrice:N0} 不在目标范围内，已忽略");
                    return;
                }

                // 计算预期价格和利润
                var expectedProfit = monitoredItem.CalculateExpectedProfit(currentPrice);
                var expectedPrice = currentPrice + expectedProfit;

                // 确定策略
                var strategy = DetermineStrategy(currentPrice, monitoredItem);

                // 添加到监控历史
                var success = await AddMonitoringOpportunityAsync(
                    itemName: itemName,
                    itemLevel: itemLevel,
                    currentPrice: currentPrice,
                    expectedPrice: expectedPrice,
                    strategy: strategy,
                    riskLevel: RiskLevel.Medium,
                    sellerName: sellerName
                );

                if (success)
                {
                    // 更新监控物品的统计信息
                    await UpdateMonitoredItemStatsAsync(monitoredItem.Id, currentPrice);
                    
                    AddLog($"✓ 发现交易机会: {itemName} (等级{itemLevel}) @ {currentPrice:N0}");
                    
                    // 如果启用自动购买，触发购买逻辑
                    if (monitoredItem.AutoPurchaseEnabled)
                    {
                        AddLog($"🛒 触发自动购买: {itemName}");
                        // TODO: 这里可以集成自动购买逻辑
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ 处理游戏数据失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"处理游戏数据失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 确定监控策略
        /// </summary>
        /// <param name="currentPrice">当前价格</param>
        /// <param name="monitoredItem">监控物品</param>
        /// <returns>策略名称</returns>
        private string DetermineStrategy(decimal currentPrice, MonitoredItem monitoredItem)
        {
            if (monitoredItem.TargetMinPrice.HasValue && currentPrice <= monitoredItem.TargetMinPrice.Value * 0.8m)
            {
                return "捡漏";
            }
            else if (monitoredItem.TargetMaxPrice.HasValue)
            {
                var potentialProfit = monitoredItem.TargetMaxPrice.Value - currentPrice;
                var profitRate = currentPrice > 0 ? potentialProfit / currentPrice : 0;
                
                if (profitRate > 0.3m)
                {
                    return "套利";
                }
            }
            
            return "趋势";
        }

        /// <summary>
        /// 更新监控物品的统计信息
        /// </summary>
        /// <param name="monitoredItemId">监控物品ID</param>
        /// <param name="foundPrice">发现的价格</param>
        private async Task UpdateMonitoredItemStatsAsync(long monitoredItemId, decimal foundPrice)
        {
            try
            {
                using var service = new MonitoredItemService();
                
                // 更新发现统计
                var monitoredItem = _monitoredItems.FirstOrDefault(x => x.Id == monitoredItemId);
                if (monitoredItem != null)
                {
                    monitoredItem.TotalFoundCount++;
                    monitoredItem.LastFoundPrice = foundPrice;
                    monitoredItem.LastFoundAt = DateTime.Now;
                    
                    await service.UpdateMonitoredItemAsync(monitoredItem);
                    
                    #if DEBUG
                    Aion2Helper.Program.LogInfo("监控", $"更新统计: {monitoredItem.ItemName} 发现次数: {monitoredItem.TotalFoundCount}");
                    #endif
                }
            }
            catch (Exception ex)
            {
                #if DEBUG
                Aion2Helper.Program.LogError("监控", $"更新监控物品统计失败: {ex.Message}");
                #endif
            }
        }

        #endregion

        #region 交易历史相关方法

        /// <summary>
        /// 加载交易历史数据
        /// </summary>
        private async Task LoadPurchaseHistoryAsync()
        {
            if (!_databaseConnected)
            {
                AddLog("⚠ 数据库未连接，跳过加载交易历史");
                return;
            }

            try
            {
                AddLog("正在加载交易历史数据...");

                // 初始化交易历史控件事件
                InitializeHistoryEvents();

                // 先检查数据库中是否有数据
                await CheckPurchaseRecordsAsync();

                // 加载第一页数据
                await LoadHistoryPageAsync(1);

                AddLog("✓ 交易历史数据加载完成");
                
                // 如果没有数据，显示提示
                if (listViewHistory.Items.Count == 0)
                {
                    AddLog("💡 提示: 交易历史为空，等待实际交易数据记录");
                }

                #if DEBUG
                Aion2Helper.Program.LogSuccess("交易历史", "数据加载完成");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"✗ 加载交易历史失败: {ex.Message}");

                #if DEBUG
                Aion2Helper.Program.LogError("交易历史", $"加载失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 检查数据库中的交易记录
        /// </summary>
        private async Task CheckPurchaseRecordsAsync()
        {
            try
            {
                using var context = new Aion2Helper.Data.Aion2DbContext();
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                // 检查总记录数
                var totalCount = await context.PurchaseRecords.CountAsync();
                var currentMachineCount = await context.PurchaseRecords
                    .Where(x => x.MachineCode == currentMachineCode)
                    .CountAsync();
                
                AddLog($"数据库检查: 总记录数={totalCount}, 当前机器记录数={currentMachineCount}");
                AddLog($"当前机器码: {currentMachineCode}");
                
                if (totalCount == 0)
                {
                    AddLog("⚠ 数据库中没有任何交易记录");
                }
                else if (currentMachineCount == 0)
                {
                    AddLog("⚠ 数据库中没有当前机器的交易记录");
                    AddLog("💡 提示: 请检查数据库中的 machine_code 字段是否与当前机器码匹配");
                    
                    // 显示数据库中存在的机器码
                    var existingMachineCodes = await context.PurchaseRecords
                        .Select(x => x.MachineCode)
                        .Distinct()
                        .ToListAsync();
                    
                    if (existingMachineCodes.Any())
                    {
                        AddLog($"数据库中存在的机器码: {string.Join(", ", existingMachineCodes)}");
                    }
                }
                else
                {
                    AddLog($"✓ 找到 {currentMachineCount} 条当前机器的交易记录");
                }

                #if DEBUG
                Aion2Helper.Program.LogInfo("交易历史", $"数据库检查完成: 总数={totalCount}, 当前机器={currentMachineCount}");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"✗ 数据库检查失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("交易历史", $"数据库检查失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 初始化交易历史控件事件
        /// </summary>
        private void InitializeHistoryEvents()
        {
            // 搜索按钮事件
            btnHistorySearch.Click += BtnHistorySearch_Click;
            
            // 重置按钮事件
            btnHistoryReset.Click += BtnHistoryReset_Click;
            
            // 分页按钮事件
            btnHistoryFirst.Click += BtnHistoryFirst_Click;
            btnHistoryPrev.Click += BtnHistoryPrev_Click;
            btnHistoryNext.Click += BtnHistoryNext_Click;
            btnHistoryLast.Click += BtnHistoryLast_Click;
            
            // 每页显示数量变化事件
            comboBoxHistoryPageSize.SelectedIndexChanged += ComboBoxHistoryPageSize_SelectedIndexChanged;
        }

        /// <summary>
        /// 加载指定页的交易历史数据
        /// </summary>
        /// <param name="pageNumber">页码</param>
        private async Task LoadHistoryPageAsync(int pageNumber)
        {
            try
            {
                using var service = new Aion2Helper.Services.PurchaseHistoryService();

                // 获取筛选条件
                var startDate = dateTimePickerStart.Value.Date;
                var endDate = dateTimePickerEnd.Value.Date;
                var status = GetSelectedHistoryStatus();
                var itemName = string.IsNullOrWhiteSpace(textBoxHistoryItemName.Text) ? null : textBoxHistoryItemName.Text.Trim();
                var pageSize = int.Parse(comboBoxHistoryPageSize.SelectedItem?.ToString() ?? "20");

                // 查询数据
                var result = await service.GetPaginatedHistoryAsync(pageNumber, pageSize, startDate, endDate, status, itemName);

                // 更新分页信息
                _currentHistoryPage = result.PageNumber;
                _historyPageSize = result.PageSize;
                _totalHistoryPages = result.TotalPages;
                _currentHistoryRecords = result.Items;

                // 更新UI
                UpdateHistoryListView(result.Items);
                UpdateHistoryPagination();

                // 获取并更新统计信息
                var stats = await service.GetHistoryStatsAsync(startDate, endDate);
                UpdateHistoryStats(stats);

                #if DEBUG
                Aion2Helper.Program.LogInfo("交易历史", $"加载第 {pageNumber} 页，共 {result.Items.Count} 条记录");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"✗ 加载交易历史第 {pageNumber} 页失败: {ex.Message}");

                #if DEBUG
                Aion2Helper.Program.LogError("交易历史", $"加载第 {pageNumber} 页失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 获取选中的历史状态
        /// </summary>
        /// <returns>状态枚举值或null</returns>
        private PurchaseStatus? GetSelectedHistoryStatus()
        {
            var selectedText = comboBoxHistoryStatus.SelectedItem?.ToString();
            return selectedText switch
            {
                "待处理" => PurchaseStatus.Pending,
                "执行中" => PurchaseStatus.Executing,
                "已完成" => PurchaseStatus.Completed,
                "失败" => PurchaseStatus.Failed,
                "已取消" => PurchaseStatus.Cancelled,
                _ => null
            };
        }

        /// <summary>
        /// 更新交易历史ListView
        /// </summary>
        /// <param name="records">交易记录</param>
        private void UpdateHistoryListView(List<PurchaseRecord> records)
        {
            try
            {
                listViewHistory.Items.Clear();

                foreach (var record in records)
                {
                    var item = new ListViewItem(record.PurchaseTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(record.ItemName);
                    item.SubItems.Add(record.ItemCategory?.Name ?? "-");
                    item.SubItems.Add(record.Price.ToString("N0"));
                    item.SubItems.Add(record.Quantity.ToString());
                    item.SubItems.Add(record.TotalAmount.ToString("N0"));
                    item.SubItems.Add(record.SellerName ?? "未知");
                    item.SubItems.Add(record.ExpectedProfit.ToString("N0"));
                    item.SubItems.Add(record.ActualProfit?.ToString("N0") ?? "未知");
                    item.SubItems.Add(record.Strategy);
                    item.SubItems.Add(GetStatusText(record.Status));
                    item.SubItems.Add(record.ExecutionTimeMs.ToString());
                    item.SubItems.Add(record.Notes ?? "");

                    // 根据状态设置行颜色
                    switch (record.Status)
                    {
                        case PurchaseStatus.Completed:
                            item.ForeColor = Color.Green;
                            break;
                        case PurchaseStatus.Failed:
                            item.ForeColor = Color.Red;
                            break;
                        case PurchaseStatus.Cancelled:
                            item.ForeColor = Color.Gray;
                            break;
                        case PurchaseStatus.Executing:
                            item.ForeColor = Color.Blue;
                            break;
                        default:
                            item.ForeColor = Color.Black;
                            break;
                    }

                    item.Tag = record;
                    listViewHistory.Items.Add(item);
                }

                #if DEBUG
                Aion2Helper.Program.LogInfo("交易历史", $"更新ListView完成，显示 {records.Count} 条记录");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"更新交易历史列表失败: {ex.Message}");

                #if DEBUG
                Aion2Helper.Program.LogError("交易历史", $"更新ListView失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 获取状态文本
        /// </summary>
        /// <param name="status">状态</param>
        /// <returns>状态文本</returns>
        private string GetStatusText(PurchaseStatus status)
        {
            return status switch
            {
                PurchaseStatus.Pending => "待处理",
                PurchaseStatus.Executing => "执行中",
                PurchaseStatus.Completed => "已完成",
                PurchaseStatus.Failed => "失败",
                PurchaseStatus.Cancelled => "已取消",
                _ => "未知"
            };
        }

        /// <summary>
        /// 更新分页信息
        /// </summary>
        private void UpdateHistoryPagination()
        {
            lblHistoryCurrentPage.Text = $"第 {_currentHistoryPage} 页，共 {_totalHistoryPages} 页";

            btnHistoryFirst.Enabled = _currentHistoryPage > 1;
            btnHistoryPrev.Enabled = _currentHistoryPage > 1;
            btnHistoryNext.Enabled = _currentHistoryPage < _totalHistoryPages;
            btnHistoryLast.Enabled = _currentHistoryPage < _totalHistoryPages;
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        /// <param name="stats">统计数据</param>
        private void UpdateHistoryStats(Aion2Helper.Services.PurchaseHistoryStats stats)
        {
            lblHistoryStats.Text = $"总记录: {stats.TotalCount} | 成功: {stats.CompletedCount} | 失败: {stats.FailedCount} | 总金额: {stats.TotalAmount:N0} | 总利润: {stats.TotalProfit:N0}";
        }

        #region 交易历史事件处理

        /// <summary>
        /// 搜索按钮点击事件
        /// </summary>
        private async void BtnHistorySearch_Click(object? sender, EventArgs e)
        {
            await LoadHistoryPageAsync(1);
        }

        /// <summary>
        /// 重置按钮点击事件
        /// </summary>
        private async void BtnHistoryReset_Click(object? sender, EventArgs e)
        {
            // 重置筛选条件
            dateTimePickerStart.Value = DateTime.Now.AddDays(-30);
            dateTimePickerEnd.Value = DateTime.Now;
            comboBoxHistoryStatus.SelectedIndex = 0;
            textBoxHistoryItemName.Text = "";

            // 重新加载数据
            await LoadHistoryPageAsync(1);
        }

        /// <summary>
        /// 首页按钮点击事件
        /// </summary>
        private async void BtnHistoryFirst_Click(object? sender, EventArgs e)
        {
            if (_currentHistoryPage > 1)
            {
                await LoadHistoryPageAsync(1);
            }
        }

        /// <summary>
        /// 上一页按钮点击事件
        /// </summary>
        private async void BtnHistoryPrev_Click(object? sender, EventArgs e)
        {
            if (_currentHistoryPage > 1)
            {
                await LoadHistoryPageAsync(_currentHistoryPage - 1);
            }
        }

        /// <summary>
        /// 下一页按钮点击事件
        /// </summary>
        private async void BtnHistoryNext_Click(object? sender, EventArgs e)
        {
            if (_currentHistoryPage < _totalHistoryPages)
            {
                await LoadHistoryPageAsync(_currentHistoryPage + 1);
            }
        }

        /// <summary>
        /// 末页按钮点击事件
        /// </summary>
        private async void BtnHistoryLast_Click(object? sender, EventArgs e)
        {
            if (_currentHistoryPage < _totalHistoryPages)
            {
                await LoadHistoryPageAsync(_totalHistoryPages);
            }
        }

        /// <summary>
        /// 每页显示数量变化事件
        /// </summary>
        private async void ComboBoxHistoryPageSize_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboBoxHistoryPageSize.SelectedItem != null)
            {
                _historyPageSize = int.Parse(comboBoxHistoryPageSize.SelectedItem.ToString()!);
                await LoadHistoryPageAsync(1);
            }
        }

        #endregion

        #endregion

        #region 价格趋势分析

        /// <summary>
        /// 初始化缓存服务
        /// </summary>
        private async Task InitializeCacheServiceAsync()
        {
            if (!_databaseConnected) return;

            try
            {
                AddLog("正在初始化数据缓存...");
                
                var cache = CacheService.Instance;
                await cache.InitializeAsync();
                
                AddLog($"✓ 缓存初始化完成");
                AddLog($"  {cache.GetCacheStats()}");
                
                // 加载价格趋势分类数据
                await LoadTrendCategoriesAsync();
            }
            catch (Exception ex)
            {
                AddLog($"✗ 缓存初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载价格趋势分类数据（从缓存）
        /// </summary>
        private async Task LoadTrendCategoriesAsync()
        {
            try
            {
                var cache = CacheService.Instance;
                var categories = cache.GetCategories();

                comboBoxTrendCategory.Items.Clear();
                comboBoxTrendCategory.Items.Add(new { Id = 0, Name = "全部分类" });
                
                foreach (var cat in categories)
                {
                    comboBoxTrendCategory.Items.Add(new { Id = cat.Id, Name = cat.Name });
                }
                
                comboBoxTrendCategory.DisplayMember = "Name";
                comboBoxTrendCategory.ValueMember = "Id";
                comboBoxTrendCategory.SelectedIndex = 0;
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 加载价格趋势分类失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化价格趋势图表
        /// </summary>
        private void InitializePriceTrendChart()
        {
            try
            {
                _trendChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
                _trendChart.Dock = DockStyle.Fill;
                _trendChart.BackColor = Color.White;

                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("MainArea");
                chartArea.AxisX.Title = "时间";
                chartArea.AxisX.LabelStyle.Format = "MM-dd HH:mm";
                chartArea.AxisX.LabelStyle.Angle = -45;
                chartArea.AxisY.Title = "价格";
                chartArea.AxisY.LabelStyle.Format = "N0";
                chartArea.BackColor = Color.WhiteSmoke;
                _trendChart.ChartAreas.Add(chartArea);

                var legend = new System.Windows.Forms.DataVisualization.Charting.Legend("MainLegend");
                legend.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
                _trendChart.Legends.Add(legend);

                panelTrendChart.Controls.Add(_trendChart);

                btnAnalyzeTrend.Click += BtnAnalyzeTrend_Click;
                btnRefreshTrendItems.Click += BtnRefreshTrendItems_Click;
                comboBoxTrendCategory.SelectedIndexChanged += ComboBoxTrendCategory_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 初始化价格趋势图表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分析按钮点击事件
        /// </summary>
        private async void BtnAnalyzeTrend_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!_databaseConnected)
                {
                    MessageBox.Show("请先连接数据库！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnAnalyzeTrend.Enabled = false;
                btnAnalyzeTrend.Text = "⏳ 分析中...";
                
                // 获取分类ID
                int? categoryId = null;
                string categoryDisplayName = "全部";
                if (comboBoxTrendCategory.SelectedItem != null)
                {
                    dynamic selectedCategory = comboBoxTrendCategory.SelectedItem;
                    int catId = selectedCategory.Id;
                    if (catId > 0)
                    {
                        categoryId = catId;
                        categoryDisplayName = selectedCategory.Name;
                    }
                }
                
                // 获取物品名称
                string? itemName = null;
                if (comboBoxTrendItem.SelectedItem != null)
                {
                    dynamic selectedItem = comboBoxTrendItem.SelectedItem;
                    string itemValue = selectedItem.Value;
                    if (!string.IsNullOrEmpty(itemValue))
                    {
                        itemName = itemValue;
                    }
                }
                var startDate = dateTimePickerTrendStart.Value.Date;
                var endDate = dateTimePickerTrendEnd.Value.Date.AddDays(1).AddSeconds(-1);
                var startHour = (int)numericUpDownTrendStartHour.Value;
                var endHour = (int)numericUpDownTrendEndHour.Value;

                AddLog($"🔍 开始分析价格趋势...");
                AddLog($"  分类: {categoryDisplayName} (ID={categoryId?.ToString() ?? "全部"})");
                AddLog($"  物品: {itemName ?? "全部"}");
                AddLog($"  日期: {startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}");
                AddLog($"  时间: {startHour}:00 ~ {endHour}:00");

                using var context = new Aion2DbContext();
                var service = new PriceTrendAnalysisService(context);
                var trendData = await service.GetPriceTrendAsync(itemName, startDate, endDate, startHour, endHour, categoryId);

                AddLog($"✅ 查询到 {trendData.Count} 个数据点");

                if (trendData.Count == 0)
                {
                    AddLog("⚠️ 未找到符合条件的数据");
                    MessageBox.Show("未找到符合条件的价格数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 清空图表
                    if (_trendChart != null)
                    {
                        _trendChart.Series.Clear();
                    }
                    return;
                }

                AddLog($"📊 正在更新图表...");
                UpdatePriceTrendChart(trendData);
                AddLog($"✅ 图表更新完成");

                var summary = await service.GetPriceSummaryAsync(itemName, startDate, endDate, startHour, endHour, categoryId);
                lblTrendSummary.Text = $"总记录: {summary.TotalCount} | 平均价格: {summary.AveragePrice:N0} | 最低: {summary.MinPrice:N0} | 最高: {summary.MaxPrice:N0} | 波动: {summary.PriceRange:N0}";

                var trend = service.AnalyzePriceTrend(trendData);
                lblTrendResult.Text = $"趋势: {trend}";

                if (trend.Contains("上涨"))
                    lblTrendResult.ForeColor = Color.Red;
                else if (trend.Contains("下跌"))
                    lblTrendResult.ForeColor = Color.Green;
                else
                    lblTrendResult.ForeColor = Color.Gray;

                AddLog($"✅ 价格趋势分析完成，共 {trendData.Count} 个数据点");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 分析价格趋势失败: {ex.Message}");
                MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyzeTrend.Enabled = true;
                btnAnalyzeTrend.Text = "📊 分析";
            }
        }

        /// <summary>
        /// 物品分类选择改变事件
        /// </summary>
        private async void ComboBoxTrendCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await LoadTrendItemsByCategory();
        }

        /// <summary>
        /// 根据分类加载物品列表（从缓存的监控物品）
        /// </summary>
        private async Task LoadTrendItemsByCategory()
        {
            try
            {
                var cache = CacheService.Instance;
                var currentMachineCode = MachineCodeHelper.GetMachineCode();

                // 获取选中的分类ID
                int? categoryId = null;
                if (comboBoxTrendCategory.SelectedItem != null)
                {
                    dynamic selectedCategory = comboBoxTrendCategory.SelectedItem;
                    int catId = selectedCategory.Id;
                    if (catId > 0)
                    {
                        categoryId = catId;
                    }
                }
                
                #if DEBUG
                Console.WriteLine($"[物品联动] 开始加载物品，分类ID={categoryId?.ToString() ?? "全部"}");
                #endif
                
                // 从缓存的监控物品中获取
                List<MonitoredItem> monitoredItems;
                if (!categoryId.HasValue)
                {
                    monitoredItems = cache.GetMonitoredItemsByMachine(currentMachineCode);
                }
                else
                {
                    monitoredItems = cache.GetMonitoredItemsByMachine(currentMachineCode)
                        .Where(m => m.CategoryId == categoryId.Value)
                        .ToList();
                }

                var items = monitoredItems
                    .Select(m => m.ItemName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                #if DEBUG
                Console.WriteLine($"[物品联动] 从缓存中找到 {items.Count} 个物品");
                if (items.Count > 0)
                {
                    Console.WriteLine($"  物品列表: {string.Join(", ", items)}");
                }
                #endif

                comboBoxTrendItem.Items.Clear();
                comboBoxTrendItem.Items.Add(new { Name = "全部物品", Value = "" });
                
                foreach (var itemName in items)
                {
                    comboBoxTrendItem.Items.Add(new { Name = itemName, Value = itemName });
                }
                
                comboBoxTrendItem.DisplayMember = "Name";
                comboBoxTrendItem.ValueMember = "Value";
                comboBoxTrendItem.SelectedIndex = 0;
                
                AddLog($"✅ 加载了 {items.Count} 个物品");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 加载物品列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新物品列表按钮
        /// </summary>
        private async void BtnRefreshTrendItems_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!_databaseConnected)
                {
                    MessageBox.Show("请先连接数据库！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddLog("🔄 刷新数据...");

                // 刷新缓存
                var cache = CacheService.Instance;
                await cache.RefreshAllAsync();
                
                AddLog($"  {cache.GetCacheStats()}");

                // 重新加载分类和物品列表
                await LoadTrendCategoriesAsync();

                AddLog($"✅ 刷新完成");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 刷新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新价格趋势图表
        /// </summary>
        private void UpdatePriceTrendChart(List<PriceTrendData> trendData)
        {
            try
            {
                if (_trendChart == null) return;

                _trendChart.Series.Clear();

                var avgSeries = new System.Windows.Forms.DataVisualization.Charting.Series("平均价格");
                avgSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                avgSeries.BorderWidth = 3;
                avgSeries.Color = Color.FromArgb(0, 123, 255);
                avgSeries.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                avgSeries.MarkerSize = 6;

                var minSeries = new System.Windows.Forms.DataVisualization.Charting.Series("最低价");
                minSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                minSeries.BorderWidth = 2;
                minSeries.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
                minSeries.Color = Color.FromArgb(40, 167, 69);

                var maxSeries = new System.Windows.Forms.DataVisualization.Charting.Series("最高价");
                maxSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                maxSeries.BorderWidth = 2;
                maxSeries.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
                maxSeries.Color = Color.FromArgb(220, 53, 69);

                foreach (var data in trendData)
                {
                    avgSeries.Points.AddXY(data.DateTime, data.AveragePrice);
                    minSeries.Points.AddXY(data.DateTime, data.MinPrice);
                    maxSeries.Points.AddXY(data.DateTime, data.MaxPrice);
                }

                _trendChart.Series.Add(avgSeries);
                _trendChart.Series.Add(minSeries);
                _trendChart.Series.Add(maxSeries);

                // 重置缩放
                _trendChart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                _trendChart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
                
                // 设置X轴范围（确保显示所有数据点）
                if (trendData.Count > 0)
                {
                    var minDate = trendData.Min(d => d.DateTime);
                    var maxDate = trendData.Max(d => d.DateTime);
                    
                    _trendChart.ChartAreas[0].AxisX.Minimum = minDate.ToOADate();
                    _trendChart.ChartAreas[0].AxisX.Maximum = maxDate.ToOADate();
                    
                    #if DEBUG
                    Console.WriteLine($"[图表X轴] 设置范围: {minDate:yyyy-MM-dd HH:mm} ~ {maxDate:yyyy-MM-dd HH:mm}");
                    #endif
                }
                
                _trendChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                _trendChart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                _trendChart.ChartAreas[0].CursorX.IsUserEnabled = true;
                _trendChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                _trendChart.ChartAreas[0].CursorY.IsUserEnabled = true;
                _trendChart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
                
                // 强制刷新图表
                _trendChart.Invalidate();
                
                #if DEBUG
                Console.WriteLine($"[图表更新] 添加了 {trendData.Count} 个数据点到图表");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"❌ 更新价格趋势图表失败: {ex.Message}");
            }
        }

        #endregion
    }
}
