using System;
using System.Windows.Forms;
using Aion2Helper.Models;

namespace Aion2Helper.Forms;

/// <summary>
/// 监控物品编辑对话框
/// </summary>
public partial class MonitoredItemEditForm : Form
{
    private MonitoredItem _item;
    private bool _isNewItem;

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
        if (_isNewItem && !string.IsNullOrEmpty(itemName))
        {
            txtItemName.Text = itemName;
        }
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
            Category = item.Category,
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
    /// 加载默认值
    /// </summary>
    private void LoadDefaultValues()
    {
        txtItemName.Text = "";
        txtCategory.Text = "未分类";
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
        txtItemName.Text = _item.ItemName;
        txtCategory.Text = _item.Category;
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
        if (string.IsNullOrWhiteSpace(txtItemName.Text))
        {
            MessageBox.Show("请输入物品名称！", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtItemName.Focus();
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
        _item.ItemName = txtItemName.Text.Trim();
        _item.Category = txtCategory.Text.Trim();
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
