using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Services;

namespace Aion2Helper.Admin
{
    /// <summary>
    /// 监控物品编辑窗体（管理端）
    /// </summary>
    public class MonitoredItemEditForm : Form
    {
        private readonly MonitoredItem? _item;
        private readonly bool _isEditMode;
        
        /// <summary>
        /// 编辑后的监控物品
        /// </summary>
        public MonitoredItem? EditedItem { get; private set; }

        private ComboBox cboItemName;
        private ComboBox cboMachineCode;
        private ComboBox cboCategory;
        private NumericUpDown nudItemLevel;
        private NumericUpDown nudTargetMinPrice;
        private NumericUpDown nudTargetMaxPrice;
        private NumericUpDown nudPriority;
        private CheckBox chkIsEnabled;
        private CheckBox chkAutoPurchaseEnabled;
        private ComboBox cboMonitorStrategy;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public MonitoredItemEditForm(MonitoredItem? item = null)
        {
            _item = item;
            _isEditMode = item != null;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "编辑监控物品" : "添加监控物品";
            this.Size = new Size(600, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int labelX = 20;
            int controlX = 150;
            int controlWidth = 400;
            int rowHeight = 45;
            int currentY = 20;

            // 如果是编辑模式，显示ID和时间信息
            if (_isEditMode && _item != null)
            {
                var infoPanel = new Panel
                {
                    Location = new Point(10, currentY),
                    Size = new Size(560, 80),
                    BackColor = Color.FromArgb(240, 248, 255),
                    BorderStyle = BorderStyle.FixedSingle
                };
                
                var lblId = new Label { Text = $"ID: {_item.Id}", Location = new Point(10, 10), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
                var lblCreated = new Label { Text = $"创建时间: {_item.CreatedAt:yyyy-MM-dd HH:mm:ss}", Location = new Point(10, 35), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9) };
                var lblUpdated = new Label { Text = $"更新时间: {_item.UpdatedAt:yyyy-MM-dd HH:mm:ss}", Location = new Point(10, 55), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9) };
                
                infoPanel.Controls.AddRange(new Control[] { lblId, lblCreated, lblUpdated });
                this.Controls.Add(infoPanel);
                currentY += 90;
            }

            // 机器码（下拉选择）
            var lblMachineCode = new Label { Text = "机器码:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            cboMachineCode = new ComboBox 
            { 
                Location = new Point(controlX, currentY), 
                Width = controlWidth, 
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Font = new Font("Microsoft YaHei UI", 10) 
            };
            this.Controls.AddRange(new Control[] { lblMachineCode, cboMachineCode });
            currentY += rowHeight;

            // 物品分类（先显示分类）
            var lblCategory = new Label { Text = "物品分类:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            cboCategory = new ComboBox { Location = new Point(controlX, currentY), Width = controlWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblCategory, cboCategory });
            currentY += rowHeight;

            // 物品名称（后显示名称）
            var lblItemName = new Label { Text = "物品名称:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            cboItemName = new ComboBox 
            { 
                Location = new Point(controlX, currentY), 
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Font = new Font("Microsoft YaHei UI", 10) 
            };
            this.Controls.AddRange(new Control[] { lblItemName, cboItemName });
            currentY += rowHeight;

            // 物品等级
            var lblLevel = new Label { Text = "物品等级:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            nudItemLevel = new NumericUpDown { Location = new Point(controlX, currentY), Width = 150, Minimum = 0, Maximum = 999, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblLevel, nudItemLevel });
            currentY += rowHeight;

            // 最低价格
            var lblMinPrice = new Label { Text = "最低价格:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            nudTargetMinPrice = new NumericUpDown { Location = new Point(controlX, currentY), Width = 200, Minimum = 0, Maximum = 999999999, DecimalPlaces = 0, ThousandsSeparator = true, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblMinPrice, nudTargetMinPrice });
            currentY += rowHeight;

            // 最高价格
            var lblMaxPrice = new Label { Text = "最高价格:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            nudTargetMaxPrice = new NumericUpDown { Location = new Point(controlX, currentY), Width = 200, Minimum = 0, Maximum = 999999999, DecimalPlaces = 0, ThousandsSeparator = true, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblMaxPrice, nudTargetMaxPrice });
            currentY += rowHeight;

            // 优先级
            var lblPriority = new Label { Text = "优先级:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            nudPriority = new NumericUpDown { Location = new Point(controlX, currentY), Width = 150, Minimum = 1, Maximum = 10, Value = 5, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblPriority, nudPriority });
            currentY += rowHeight;

            // 启用监控
            chkIsEnabled = new CheckBox { Text = "启用监控", Location = new Point(controlX, currentY), AutoSize = true, Checked = true, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.Add(chkIsEnabled);
            currentY += 35;

            // 自动购买
            chkAutoPurchaseEnabled = new CheckBox { Text = "启用自动购买", Location = new Point(controlX, currentY), AutoSize = true, Checked = false, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.Add(chkAutoPurchaseEnabled);
            currentY += 35;

            // 监控策略（下拉选择）
            var lblStrategy = new Label { Text = "监控策略:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            cboMonitorStrategy = new ComboBox 
            { 
                Location = new Point(controlX, currentY), 
                Width = controlWidth, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft YaHei UI", 10) 
            };
            this.Controls.AddRange(new Control[] { lblStrategy, cboMonitorStrategy });
            currentY += rowHeight;

            // 备注
            var lblNotes = new Label { Text = "备注:", Location = new Point(labelX, currentY + 5), AutoSize = true, Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold) };
            txtNotes = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth, Height = 80, Multiline = true, Font = new Font("Microsoft YaHei UI", 10) };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            currentY += 90;

            // 按钮
            btnSave = new Button 
            { 
                Text = "保存", 
                Location = new Point(controlX + 200, currentY), 
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button 
            { 
                Text = "取消", 
                Location = new Point(controlX + 310, currentY), 
                Size = new Size(100, 40),
                Font = new Font("Microsoft YaHei UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void LoadData()
        {
            try
            {
                var cache = CacheService.Instance;
                
                // 1. 加载机器码列表（从缓存）
                var machines = cache.GetEnabledMachines();
                cboMachineCode.Items.Clear();
                foreach (var machine in machines)
                {
                    var displayName = string.IsNullOrEmpty(machine.MachineName)
                        ? machine.MachineCode
                        : $"{machine.MachineName} ({machine.MachineCode.Substring(0, 8)}...)";
                    cboMachineCode.Items.Add(new MachineCodeItem 
                    { 
                        DisplayName = displayName,
                        MachineCode = machine.MachineCode 
                    });
                }
                cboMachineCode.DisplayMember = "DisplayName";
                
                // 2. 加载物品名称列表（从缓存）
                var items = cache.GetItems().Where(x => x.IsEnabled).OrderBy(x => x.Name).ToList();
                cboItemName.Items.Clear();
                foreach (var item in items)
                {
                    cboItemName.Items.Add(item.Name);
                }
                
                // 3. 加载分类列表（从缓存）
                var categories = cache.GetCategories();
                cboCategory.Items.Clear();
                cboCategory.Items.Add("未分类");
                cboCategory.Items.AddRange(categories.ToArray());
                cboCategory.DisplayMember = "Name";
                cboCategory.ValueMember = "Id";
                
                // 4. 加载监控策略列表
                var strategies = MonitorStrategyHelper.GetAllStrategies();
                cboMonitorStrategy.Items.Clear();
                cboMonitorStrategy.Items.AddRange(strategies.ToArray());
                cboMonitorStrategy.DisplayMember = "Description";
                cboMonitorStrategy.SelectedIndex = 0; // 默认选择第一个（价格监控）

                // 如果是编辑模式，填充数据
                if (_isEditMode && _item != null)
                {
                    // 设置机器码
                    var machineItem = cboMachineCode.Items.Cast<MachineCodeItem>()
                        .FirstOrDefault(x => x.MachineCode == _item.MachineCode);
                    if (machineItem != null)
                    {
                        cboMachineCode.SelectedItem = machineItem;
                    }
                    else
                    {
                        cboMachineCode.Text = _item.MachineCode; // 如果找不到，直接显示
                    }
                    
                    // 设置物品名称
                    if (cboItemName.Items.Contains(_item.ItemName))
                    {
                        cboItemName.SelectedItem = _item.ItemName;
                    }
                    else
                    {
                        cboItemName.Text = _item.ItemName; // 如果找不到，直接显示
                    }
                    
                    nudItemLevel.Value = _item.ItemLevel ?? 0;
                    nudTargetMinPrice.Value = _item.TargetMinPrice ?? 0;
                    nudTargetMaxPrice.Value = _item.TargetMaxPrice ?? 0;
                    nudPriority.Value = _item.Priority;
                    chkIsEnabled.Checked = _item.IsEnabled;
                    chkAutoPurchaseEnabled.Checked = _item.AutoPurchaseEnabled;
                    
                    // 设置监控策略
                    if (!string.IsNullOrEmpty(_item.MonitorStrategy))
                    {
                        // 尝试解析策略
                        var strategy = MonitorStrategyHelper.ParseStrategy(_item.MonitorStrategy);
                        if (strategy.HasValue)
                        {
                            var strategyItem = strategies.FirstOrDefault(s => s.Strategy == strategy.Value);
                            if (strategyItem != null)
                            {
                                cboMonitorStrategy.SelectedItem = strategyItem;
                            }
                        }
                    }
                    
                    txtNotes.Text = _item.Notes ?? string.Empty;
                    
                    // 设置分类选中项
                    if (_item.CategoryId.HasValue)
                    {
                        var selectedCategory = categories.FirstOrDefault(c => c.Id == _item.CategoryId.Value);
                        if (selectedCategory != null)
                        {
                            cboCategory.SelectedItem = selectedCategory;
                        }
                        else
                        {
                            cboCategory.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        cboCategory.SelectedIndex = 0;
                    }
                }
                else
                {
                    // 添加模式，默认选择
                    if (cboMachineCode.Items.Count > 0)
                        cboMachineCode.SelectedIndex = 0;
                    cboCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 机器码下拉列表项
        /// </summary>
        private class MachineCodeItem
        {
            public string DisplayName { get; set; } = string.Empty;
            public string MachineCode { get; set; } = string.Empty;
            
            public override string ToString() => DisplayName;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // 验证输入
            var machineCode = string.Empty;
            if (cboMachineCode.SelectedItem is MachineCodeItem machineItem)
            {
                machineCode = machineItem.MachineCode;
            }
            else if (!string.IsNullOrWhiteSpace(cboMachineCode.Text))
            {
                machineCode = cboMachineCode.Text.Trim();
            }
            
            if (string.IsNullOrWhiteSpace(machineCode))
            {
                MessageBox.Show("请选择或输入机器码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboMachineCode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(cboItemName.Text))
            {
                MessageBox.Show("请选择或输入物品名称！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboItemName.Focus();
                return;
            }

            if (nudTargetMinPrice.Value > 0 && nudTargetMaxPrice.Value > 0 && nudTargetMinPrice.Value >= nudTargetMaxPrice.Value)
            {
                MessageBox.Show("最低价格必须小于最高价格！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSave.Enabled = false;
                using var context = new Aion2DbContext();
                var service = new MonitoredItemService(context);

                MonitoredItem item;
                if (_isEditMode && _item != null)
                {
                    item = _item;
                }
                else
                {
                    item = new MonitoredItem();
                }

                // 填充数据
                item.MachineCode = machineCode;
                item.ItemName = cboItemName.Text.Trim();
                item.ItemLevel = nudItemLevel.Value > 0 ? (int)nudItemLevel.Value : null;
                item.TargetMinPrice = nudTargetMinPrice.Value > 0 ? nudTargetMinPrice.Value : null;
                item.TargetMaxPrice = nudTargetMaxPrice.Value > 0 ? nudTargetMaxPrice.Value : null;
                item.Priority = (int)nudPriority.Value;
                item.IsEnabled = chkIsEnabled.Checked;
                item.AutoPurchaseEnabled = chkAutoPurchaseEnabled.Checked;
                
                // 设置监控策略（从下拉框获取）
                if (cboMonitorStrategy.SelectedItem is MonitorStrategyItem strategyItem)
                {
                    item.MonitorStrategy = strategyItem.Strategy.GetDisplayName();
                }
                else
                {
                    item.MonitorStrategy = "价格监控"; // 默认值
                }
                
                item.Notes = txtNotes.Text.Trim();
                
                if (cboCategory.SelectedIndex > 0 && cboCategory.SelectedValue is int categoryId)
                {
                    item.CategoryId = categoryId;
                }
                else
                {
                    item.CategoryId = null;
                }

                if (_isEditMode)
                {
                    EditedItem = await service.UpdateMonitoredItemAsync(item);
                }
                else
                {
                    EditedItem = await service.AddMonitoredItemAsync(item);
                }

                MessageBox.Show("保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
