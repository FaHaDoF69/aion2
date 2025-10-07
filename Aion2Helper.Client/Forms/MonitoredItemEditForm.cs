using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aion2Helper.Models;
using Aion2Helper.Services;
using Aion2Helper.Data;

namespace Aion2Helper.Forms;

/// <summary>
/// 监控物品编辑对话框
/// </summary>
public partial class MonitoredItemEditForm : Form
{
    private MonitoredItem _item;
    private bool _isNewItem;
    private List<ItemCategory> _categories = new List<ItemCategory>();
    private List<Item> _allItems = new List<Item>();
    private Item? _selectedItem;

    /// <summary>
    /// 编辑后的监控物品
    /// </summary>
    public MonitoredItem EditedItem => _item;

    /// <summary>
    /// 设置预填充的物品名称
    /// </summary>
    /// <param name="itemName">物品名称</param>
    public void SetPrefilledItemName(string itemName)
    {
        // 由于现在使用下拉列表选择，不再支持预填充
        // 保留此方法以保持兼容性
    }

    /// <summary>
    /// 构造函数 - 新建物品
    /// </summary>
    public MonitoredItemEditForm()
    {
        InitializeComponent();
        _isNewItem = true;
        _item = new MonitoredItem();
        SetupForm();
        LoadCategoriesAsync().Wait();
        LoadDefaultValues();
    }

    /// <summary>
    /// 构造函数 - 编辑现有物品
    /// </summary>
    /// <param name="item">要编辑的监控物品</param>
    public MonitoredItemEditForm(MonitoredItem item)
    {
        InitializeComponent();
        _isNewItem = false;
        _item = new MonitoredItem
        {
            Id = item.Id,
            MachineCode = item.MachineCode,
            ItemName = item.ItemName,
#pragma warning disable CS0618
            Category = item.Category,
#pragma warning restore CS0618
            CategoryId = item.CategoryId,
            ItemCategory = item.ItemCategory,
            ItemLevel = item.ItemLevel,
            TargetMinPrice = item.TargetMinPrice,
            TargetMaxPrice = item.TargetMaxPrice,
            Priority = item.Priority,
            IsEnabled = item.IsEnabled,
            AutoPurchaseEnabled = item.AutoPurchaseEnabled,
            MonitorStrategy = item.MonitorStrategy,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            LastMonitoredAt = item.LastMonitoredAt,
            LastFoundAt = item.LastFoundAt,
            LastFoundPrice = item.LastFoundPrice,
            TotalFoundCount = item.TotalFoundCount,
            TotalPurchaseCount = item.TotalPurchaseCount
        };
        SetupForm();
        LoadCategoriesAsync().Wait();
        LoadItemValues();
    }

    /// <summary>
    /// 设置窗体
    /// </summary>
    private void SetupForm()
    {
        this.Text = _isNewItem ? "添加监控物品" : "编辑监控物品";
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.ShowInTaskbar = false;
    }

    /// <summary>
    /// 从缓存加载物品分类和物品数据
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            // 从缓存获取分类和物品数据
            var cacheService = CacheService.Instance;
            _categories = cacheService.GetCategories();
            _allItems = cacheService.GetItems();
            
            // 如果缓存为空，尝试重新加载
            if (_categories.Count == 0 || _allItems.Count == 0)
            {
                #if DEBUG
                Console.WriteLine("[监控物品编辑] 缓存为空，正在重新加载数据...");
                #endif
                
                await cacheService.RefreshAllAsync();
                _categories = cacheService.GetCategories();
                _allItems = cacheService.GetItems();
            }
            
            // 清空并重新填充分类下拉列表
            cmbCategory.Items.Clear();
            
            // 添加一个默认选项
            cmbCategory.Items.Add(new ComboBoxItem { Text = "-- 请选择分类 --", Value = null });
            
            // 添加所有分类
            foreach (var category in _categories)
            {
                cmbCategory.Items.Add(new ComboBoxItem 
                { 
                    Text = category.Name, 
                    Value = category.Id 
                });
            }
            
            // 默认选中第一项
            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }

            // 清空物品列表
            cmbItemName.Items.Clear();
            cmbItemName.Items.Add(new ComboBoxItem { Text = "-- 请先选择分类 --", Value = null });
            cmbItemName.SelectedIndex = 0;
            cmbItemName.Enabled = false;

            #if DEBUG
            Console.WriteLine($"[监控物品编辑] 成功从缓存加载 {_categories.Count} 个分类，{_allItems.Count} 个物品");
            #endif
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            #if DEBUG
            Console.WriteLine($"[监控物品编辑] 加载数据失败: {ex.Message}");
            #endif
        }
    }

    /// <summary>
    /// 分类选择变化事件
    /// </summary>
    private void cmbCategory_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            if (cmbCategory.SelectedItem is ComboBoxItem selectedCategory && selectedCategory.Value != null)
            {
                var categoryId = (int)selectedCategory.Value;
                
                // 加载该分类下的物品
                LoadItemsByCategory(categoryId);
            }
            else
            {
                // 未选择分类，清空物品列表
                cmbItemName.Items.Clear();
                cmbItemName.Items.Add(new ComboBoxItem { Text = "-- 请先选择分类 --", Value = null });
                cmbItemName.SelectedIndex = 0;
                cmbItemName.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            #if DEBUG
            Console.WriteLine($"[监控物品编辑] 分类选择变化处理失败: {ex.Message}");
            #endif
        }
    }

    /// <summary>
    /// 根据分类ID加载物品列表
    /// </summary>
    private void LoadItemsByCategory(int categoryId)
    {
        try
        {
            // 筛选该分类的物品
            var categoryItems = _allItems.Where(i => i.CategoryId == categoryId && i.IsEnabled).OrderBy(i => i.SortOrder).ThenBy(i => i.Name).ToList();
            
            // 清空并重新填充物品下拉列表
            cmbItemName.Items.Clear();
            
            if (categoryItems.Count > 0)
            {
                cmbItemName.Items.Add(new ComboBoxItem { Text = "-- 请选择物品 --", Value = null });
                
                foreach (var item in categoryItems)
                {
                    var displayText = item.ItemLevel.HasValue 
                        ? $"{item.Name} (等级{item.ItemLevel})" 
                        : item.Name;
                    
                    cmbItemName.Items.Add(new ComboBoxItem 
                    { 
                        Text = displayText, 
                        Value = item.Id 
                    });
                }
                
                cmbItemName.SelectedIndex = 0;
                cmbItemName.Enabled = true;
                
                #if DEBUG
                Console.WriteLine($"[监控物品编辑] 加载分类 {categoryId} 的 {categoryItems.Count} 个物品");
                #endif
            }
            else
            {
                cmbItemName.Items.Add(new ComboBoxItem { Text = "-- 该分类下无物品 --", Value = null });
                cmbItemName.SelectedIndex = 0;
                cmbItemName.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            #if DEBUG
            Console.WriteLine($"[监控物品编辑] 加载分类物品失败: {ex.Message}");
            #endif
        }
    }

    /// <summary>
    /// 物品选择变化事件
    /// </summary>
    private void cmbItemName_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            if (cmbItemName.SelectedItem is ComboBoxItem selectedItem && selectedItem.Value != null)
            {
                var itemId = (int)selectedItem.Value;
                _selectedItem = _allItems.FirstOrDefault(i => i.Id == itemId);
                
                if (_selectedItem != null)
                {
                    // 自动填充参考价格等信息
                    if (_selectedItem.ReferencePrice.HasValue && _selectedItem.ReferencePrice.Value > 0)
                    {
                        // 如果有参考价格，设置一个合理的价格区间
                        var referencePrice = _selectedItem.ReferencePrice.Value;
                        numTargetMinPrice.Value = referencePrice * 0.8m; // 低于参考价 20%
                        numTargetMaxPrice.Value = referencePrice * 1.2m; // 高于参考价 20%
                    }
                    
                    #if DEBUG
                    Console.WriteLine($"[监控物品编辑] 选择物品: {_selectedItem.Name}，参考价格: {_selectedItem.ReferencePrice}");
                    #endif
                }
            }
            else
            {
                _selectedItem = null;
            }
        }
        catch (Exception ex)
        {
            #if DEBUG
            Console.WriteLine($"[监控物品编辑] 物品选择变化处理失败: {ex.Message}");
            #endif
        }
    }

    /// <summary>
    /// 加载默认值
    /// </summary>
    private void LoadDefaultValues()
    {
        // 默认选中第一项
        if (cmbCategory.Items.Count > 0)
        {
            cmbCategory.SelectedIndex = 0;
        }
        if (cmbItemName.Items.Count > 0)
        {
            cmbItemName.SelectedIndex = 0;
        }
        numTargetMinPrice.Value = 0;
        numTargetMaxPrice.Value = 0;
        numPriority.Value = 5;
        chkIsEnabled.Checked = true;
        chkAutoPurchaseEnabled.Checked = false;
        txtMonitorStrategy.Text = "价格监控";
        txtNotes.Text = "";
    }

    /// <summary>
    /// 加载物品值
    /// </summary>
    private void LoadItemValues()
    {
        // 先设置分类
        if (_item.CategoryId.HasValue)
        {
            // 查找对应的分类ID并选中
            for (int i = 0; i < cmbCategory.Items.Count; i++)
            {
                if (cmbCategory.Items[i] is ComboBoxItem item && 
                    item.Value is int categoryId && 
                    categoryId == _item.CategoryId.Value)
                {
                    cmbCategory.SelectedIndex = i;
                    
                    // 选中分类后，加载该分类的物品
                    LoadItemsByCategory(categoryId);
                    
                    // 然后选中对应的物品
                    var matchingItem = _allItems.FirstOrDefault(it => 
                        it.Name.Equals(_item.ItemName, StringComparison.OrdinalIgnoreCase) && 
                        it.CategoryId == categoryId);
                    
                    if (matchingItem != null)
                    {
                        _selectedItem = matchingItem;
                        
                        // 在物品列表中选中它
                        for (int j = 0; j < cmbItemName.Items.Count; j++)
                        {
                            if (cmbItemName.Items[j] is ComboBoxItem itemCombo && 
                                itemCombo.Value is int itemId && 
                                itemId == matchingItem.Id)
                            {
                                cmbItemName.SelectedIndex = j;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
        else
        {
            // 如果没有分类ID，选中第一项
            cmbCategory.SelectedIndex = 0;
        }
        
        numTargetMinPrice.Value = _item.TargetMinPrice ?? 0;
        numTargetMaxPrice.Value = _item.TargetMaxPrice ?? 0;
        numPriority.Value = _item.Priority;
        chkIsEnabled.Checked = _item.IsEnabled;
        chkAutoPurchaseEnabled.Checked = _item.AutoPurchaseEnabled;
        txtMonitorStrategy.Text = _item.MonitorStrategy;
        txtNotes.Text = _item.Notes ?? "";

        // 显示统计信息（只读）
        if (!_isNewItem)
        {
            lblTotalFoundCount.Text = $"发现次数: {_item.TotalFoundCount}";
            lblTotalPurchaseCount.Text = $"购买次数: {_item.TotalPurchaseCount}";
            lblLastFoundAt.Text = _item.LastFoundAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "从未发现";
            lblLastFoundPrice.Text = _item.LastFoundPrice?.ToString("N0") ?? "无";
        }
    }

    /// <summary>
    /// 保存按钮点击事件
    /// </summary>
    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        SaveItemValues();
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    /// <summary>
    /// 验证输入
    /// </summary>
    /// <returns></returns>
    private bool ValidateInput()
    {
        if (_selectedItem == null)
        {
            MessageBox.Show("请选择要监控的物品！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbItemName.Focus();
            return false;
        }

        if (numTargetMinPrice.Value > 0 && numTargetMaxPrice.Value > 0 && numTargetMinPrice.Value >= numTargetMaxPrice.Value)
        {
            MessageBox.Show("最低价格必须小于最高价格！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            numTargetMinPrice.Focus();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 保存物品值
    /// </summary>
    private void SaveItemValues()
    {
        if (_selectedItem != null)
        {
            // 从选择的物品填充信息
            _item.ItemName = _selectedItem.Name;
            _item.CategoryId = _selectedItem.CategoryId;
            _item.ItemCategory = _selectedItem.Category;
            _item.ItemLevel = _selectedItem.ItemLevel;
        }
        
        _item.TargetMinPrice = numTargetMinPrice.Value > 0 ? numTargetMinPrice.Value : null;
        _item.TargetMaxPrice = numTargetMaxPrice.Value > 0 ? numTargetMaxPrice.Value : null;
        _item.Priority = (int)numPriority.Value;
        _item.IsEnabled = chkIsEnabled.Checked;
        _item.AutoPurchaseEnabled = chkAutoPurchaseEnabled.Checked;
        _item.MonitorStrategy = txtMonitorStrategy.Text.Trim();
        _item.Notes = txtNotes.Text.Trim();
        _item.UpdatedAt = DateTime.Now;
    }
}

/// <summary>
/// ComboBox 选项类
/// </summary>
public class ComboBoxItem
{
    public string Text { get; set; } = string.Empty;
    public object? Value { get; set; }

    public override string ToString()
    {
        return Text;
    }
}
