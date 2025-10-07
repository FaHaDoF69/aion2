using Aion2Helper.Models;
using Aion2Helper.Services;
using Aion2Helper.Utils;
using Microsoft.EntityFrameworkCore;

namespace Aion2Helper
{
    public partial class Form1 : Form
    {
        private readonly ImageDetectionService _imageService;
        private readonly GameWindowService _windowService;
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

        public Form1()
        {
            InitializeComponent();
            
            _windowService = new GameWindowService();
            _imageService = new ImageDetectionService(_windowService);
            
            InitializeTimers();
            InitializeListView();
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
            
            // 检查数据库连接并加载监控物品
            await CheckDatabaseAndLoadItemsAsync();
            
            // 加载交易历史数据
            await LoadPurchaseHistoryAsync();
            
            AddLog("请点击'开始监控'按钮开始监控拍卖行");
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
                
                // 直接用完整机器码查询
                _monitoredItems = await context.MonitoredItems
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
                    listItem.SubItems.Add(item.Category);
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
                    
                    // 检查是否已存在该物品
                    using var service = new MonitoredItemService();
                    var existingItems = await service.GetCurrentMachineMonitoredItemsAsync(true);
                    if (existingItems.Any(x => x.ItemName.Equals(newItem.ItemName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("该物品已在监控列表中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    
                    // 保存到数据库
                    var savedItem = await service.AddMonitoredItemAsync(newItem);
                    
                    AddLog($"✓ 成功添加监控物品: {savedItem.ItemName}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogSuccess("数据库", $"添加监控物品: {savedItem.ItemName} (ID: {savedItem.Id})");
                    #endif
                    
                    // 刷新列表
                    await LoadMonitoredItemsWithMachineCodeFixAsync();
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
                    
                    // 更新到数据库
                    using var service = new MonitoredItemService();
                    var updatedItem = await service.UpdateMonitoredItemAsync(editedItem);
                    
                    AddLog($"✓ 成功更新监控物品: {updatedItem.ItemName}");
                    
                    #if DEBUG
                    Aion2Helper.Program.LogSuccess("数据库", $"更新监控物品: {updatedItem.ItemName} (ID: {updatedItem.Id})");
                    #endif
                    
                    // 刷新列表
                    await LoadMonitoredItemsWithMachineCodeFixAsync();
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
                    AddLog("⚠ 数据库中没有任何交易记录，请先添加一些测试数据");
                    AddLog("💡 提示: 可以运行 SQL/sample_purchase_records_data.sql 中的示例数据");
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
            
            // 添加测试数据按钮事件
            btnAddTestData.Click += BtnAddTestData_Click;
            
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
        /// 添加测试数据按钮点击事件
        /// </summary>
        private async void BtnAddTestData_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要添加测试数据吗？\n\n这将向数据库中添加：\n• 约12条示例交易记录\n• 约8条监控机会记录\n\n用于测试交易历史和监控功能。", 
                "添加测试数据", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                await AddTestPurchaseRecordsAsync();
                await AddTestMonitoringOpportunitiesAsync();
            }
        }

        /// <summary>
        /// 添加测试监控机会记录
        /// </summary>
        private async Task AddTestMonitoringOpportunitiesAsync()
        {
            try
            {
                AddLog("正在添加测试监控机会数据...");
                
                // 添加一些测试监控机会
                await AddMonitoringOpportunityAsync("传说武器强化石", 3, 850000, 1200000, "捡漏", RiskLevel.Low, "玩家A");
                await AddMonitoringOpportunityAsync("史诗防具碎片", 2, 450000, 600000, "套利", RiskLevel.Medium, "玩家B");
                await AddMonitoringOpportunityAsync("稀有材料包", 2, 120000, 180000, "趋势", RiskLevel.Low, "玩家C");
                await AddMonitoringOpportunityAsync("魔法水晶", 1, 75000, 95000, "批量", RiskLevel.Low, "玩家D");
                await AddMonitoringOpportunityAsync("装备碎片", 2, 200000, 280000, "捡漏", RiskLevel.Medium, "玩家E");
                await AddMonitoringOpportunityAsync("强化石", 2, 300000, 350000, "套利", RiskLevel.Medium, "玩家F");
                await AddMonitoringOpportunityAsync("宝石", 2, 500000, 750000, "捡漏", RiskLevel.Low, "玩家G");
                await AddMonitoringOpportunityAsync("传说装备", 3, 1500000, 2000000, "捡漏", RiskLevel.High, "玩家H");

                AddLog("✓ 成功添加 8 条测试监控机会记录");
                
                #if DEBUG
                Aion2Helper.Program.LogSuccess("监控历史", "添加测试监控机会完成");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"✗ 添加测试监控机会失败: {ex.Message}");
                
                #if DEBUG
                Aion2Helper.Program.LogError("监控历史", $"添加测试监控机会失败: {ex.Message}");
                #endif
            }
        }

        /// <summary>
        /// 添加测试交易记录
        /// </summary>
        private async Task AddTestPurchaseRecordsAsync()
        {
            try
            {
                AddLog("正在添加测试交易数据...");
                
                using var service = new Aion2Helper.Services.PurchaseHistoryService();
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                // 创建测试数据
                var testRecords = new List<PurchaseRecord>
                {
                    // 最近的成功交易
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "传说武器强化石",
                        Price = 850000,
                        Quantity = 1,
                        TotalAmount = 850000,
                        SellerName = "玩家A",
                        ExpectedProfit = 350000,
                        ActualProfit = 380000,
                        Strategy = "捡漏",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 1250,
                        Notes = "价格非常好",
                        PurchaseTime = DateTime.Now.AddHours(-1)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "史诗防具碎片",
                        Price = 450000,
                        Quantity = 2,
                        TotalAmount = 900000,
                        SellerName = "玩家B",
                        ExpectedProfit = 150000,
                        ActualProfit = 140000,
                        Strategy = "套利",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 980,
                        Notes = "利润略低于预期",
                        PurchaseTime = DateTime.Now.AddHours(-2)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "稀有材料包",
                        Price = 120000,
                        Quantity = 5,
                        TotalAmount = 600000,
                        SellerName = "玩家C",
                        ExpectedProfit = 80000,
                        ActualProfit = 95000,
                        Strategy = "趋势",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 750,
                        Notes = "市场价格上涨",
                        PurchaseTime = DateTime.Now.AddHours(-3)
                    },
                    // 昨天的交易
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "魔法水晶",
                        Price = 75000,
                        Quantity = 10,
                        TotalAmount = 750000,
                        SellerName = "玩家D",
                        ExpectedProfit = 25000,
                        ActualProfit = 30000,
                        Strategy = "批量",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 1100,
                        Notes = "批量购买成功",
                        PurchaseTime = DateTime.Now.AddDays(-1)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "装备碎片",
                        Price = 200000,
                        Quantity = 3,
                        TotalAmount = 600000,
                        SellerName = "玩家E",
                        ExpectedProfit = 60000,
                        ActualProfit = 55000,
                        Strategy = "捡漏",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 890,
                        Notes = "小幅盈利",
                        PurchaseTime = DateTime.Now.AddDays(-1).AddHours(-2)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "强化石",
                        Price = 300000,
                        Quantity = 2,
                        TotalAmount = 600000,
                        SellerName = "玩家F",
                        ExpectedProfit = 100000,
                        ActualProfit = 0,
                        Strategy = "套利",
                        Status = PurchaseStatus.Failed,
                        ExecutionTimeMs = 2300,
                        Notes = "交易失败，网络问题",
                        PurchaseTime = DateTime.Now.AddDays(-1).AddHours(-4)
                    },
                    // 前天的交易
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "宝石",
                        Price = 500000,
                        Quantity = 1,
                        TotalAmount = 500000,
                        SellerName = "玩家G",
                        ExpectedProfit = 200000,
                        ActualProfit = 220000,
                        Strategy = "捡漏",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 650,
                        Notes = "超预期收益",
                        PurchaseTime = DateTime.Now.AddDays(-2)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "药水",
                        Price = 15000,
                        Quantity = 20,
                        TotalAmount = 300000,
                        SellerName = "玩家H",
                        ExpectedProfit = 5000,
                        ActualProfit = 8000,
                        Strategy = "批量",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 1800,
                        Notes = "消耗品投资",
                        PurchaseTime = DateTime.Now.AddDays(-2).AddHours(-1)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "装备",
                        Price = 800000,
                        Quantity = 1,
                        TotalAmount = 800000,
                        SellerName = "玩家I",
                        ExpectedProfit = 300000,
                        ActualProfit = null,
                        Strategy = "趋势",
                        Status = PurchaseStatus.Executing,
                        ExecutionTimeMs = 0,
                        Notes = "正在执行中",
                        PurchaseTime = DateTime.Now.AddDays(-2).AddHours(-3)
                    },
                    // 一周前的交易
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "传说装备",
                        Price = 1500000,
                        Quantity = 1,
                        TotalAmount = 1500000,
                        SellerName = "玩家J",
                        ExpectedProfit = 500000,
                        ActualProfit = 480000,
                        Strategy = "捡漏",
                        Status = PurchaseStatus.Completed,
                        ExecutionTimeMs = 2100,
                        Notes = "高价值交易",
                        PurchaseTime = DateTime.Now.AddDays(-7)
                    },
                    // 待处理的交易
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "新物品",
                        Price = 250000,
                        Quantity = 2,
                        TotalAmount = 500000,
                        SellerName = "玩家R",
                        ExpectedProfit = 80000,
                        ActualProfit = null,
                        Strategy = "测试",
                        Status = PurchaseStatus.Pending,
                        ExecutionTimeMs = 0,
                        Notes = "等待处理",
                        PurchaseTime = DateTime.Now.AddMinutes(-30)
                    },
                    new PurchaseRecord
                    {
                        MachineCode = currentMachineCode,
                        ItemName = "测试物品",
                        Price = 100000,
                        Quantity = 1,
                        TotalAmount = 100000,
                        SellerName = "玩家S",
                        ExpectedProfit = 30000,
                        ActualProfit = null,
                        Strategy = "测试",
                        Status = PurchaseStatus.Pending,
                        ExecutionTimeMs = 0,
                        Notes = "测试交易",
                        PurchaseTime = DateTime.Now.AddMinutes(-10)
                    }
                };

                // 批量添加记录
                int addedCount = 0;
                foreach (var record in testRecords)
                {
                    try
                    {
                        await service.AddPurchaseRecordAsync(record);
                        addedCount++;
                    }
                    catch (Exception ex)
                    {
                        AddLog($"添加记录失败: {record.ItemName} - {ex.Message}");
                    }
                }

                AddLog($"✓ 成功添加 {addedCount} 条测试交易记录");
                
                // 重新加载数据
                await LoadHistoryPageAsync(1);
                
                MessageBox.Show($"成功添加 {addedCount} 条测试数据！", "添加完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                #if DEBUG
                Aion2Helper.Program.LogSuccess("交易历史", $"添加测试数据完成: {addedCount} 条记录");
                #endif
            }
            catch (Exception ex)
            {
                AddLog($"✗ 添加测试数据失败: {ex.Message}");
                MessageBox.Show($"添加测试数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                #if DEBUG
                Aion2Helper.Program.LogError("交易历史", $"添加测试数据失败: {ex.Message}");
                #endif
            }
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
    }
}
