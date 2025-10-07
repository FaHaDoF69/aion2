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
    /// 物品编辑窗体
    /// </summary>
    public class ItemEditForm : Form
    {
        private readonly Item? _item;
        private readonly bool _isEditMode;

        private TextBox txtName;
        private ComboBox cboCategory;
        private NumericUpDown nudItemLevel;
        private TextBox txtQuality;
        private NumericUpDown nudReferencePrice;
        private TextBox txtDescription;
        private TextBox txtIconPath;
        private CheckBox chkIsTradable;
        private CheckBox chkIsEnabled;
        private NumericUpDown nudSortOrder;
        private TextBox txtRemarks;
        private Button btnSave;
        private Button btnCancel;

        public ItemEditForm(Item? item = null)
        {
            _item = item;
            _isEditMode = item != null;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _isEditMode ? "编辑物品" : "添加物品";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建表单控件
            int labelX = 20;
            int controlX = 150;
            int controlWidth = 400;
            int rowHeight = 40;
            int currentY = 20;

            // 物品名称
            var lblName = new Label { Text = "物品名称:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            txtName = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            currentY += rowHeight;

            // 物品分类
            var lblCategory = new Label { Text = "物品分类:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            cboCategory = new ComboBox { Location = new Point(controlX, currentY), Width = controlWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblCategory, cboCategory });
            currentY += rowHeight;

            // 物品等级
            var lblLevel = new Label { Text = "物品等级:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            nudItemLevel = new NumericUpDown { Location = new Point(controlX, currentY), Width = 150, Minimum = 0, Maximum = 999 };
            this.Controls.AddRange(new Control[] { lblLevel, nudItemLevel });
            currentY += rowHeight;

            // 物品品质
            var lblQuality = new Label { Text = "物品品质:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            txtQuality = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth, PlaceholderText = "如：普通、高级、稀有、史诗、传说" };
            this.Controls.AddRange(new Control[] { lblQuality, txtQuality });
            currentY += rowHeight;

            // 参考价格
            var lblPrice = new Label { Text = "参考价格:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            nudReferencePrice = new NumericUpDown { Location = new Point(controlX, currentY), Width = 200, Minimum = 0, Maximum = 999999999, DecimalPlaces = 2 };
            this.Controls.AddRange(new Control[] { lblPrice, nudReferencePrice });
            currentY += rowHeight;

            // 物品描述
            var lblDescription = new Label { Text = "物品描述:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth, Height = 80, Multiline = true };
            this.Controls.AddRange(new Control[] { lblDescription, txtDescription });
            currentY += 90;

            // 图标路径
            var lblIcon = new Label { Text = "图标路径:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            txtIconPath = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth };
            this.Controls.AddRange(new Control[] { lblIcon, txtIconPath });
            currentY += rowHeight;

            // 是否可交易
            chkIsTradable = new CheckBox { Text = "可交易", Location = new Point(controlX, currentY), AutoSize = true, Checked = true };
            this.Controls.Add(chkIsTradable);
            currentY += 30;

            // 是否启用
            chkIsEnabled = new CheckBox { Text = "启用", Location = new Point(controlX, currentY), AutoSize = true, Checked = true };
            this.Controls.Add(chkIsEnabled);
            currentY += 30;

            // 排序顺序
            var lblSortOrder = new Label { Text = "排序顺序:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            nudSortOrder = new NumericUpDown { Location = new Point(controlX, currentY), Width = 150, Minimum = 0, Maximum = 9999 };
            this.Controls.AddRange(new Control[] { lblSortOrder, nudSortOrder });
            currentY += rowHeight;

            // 备注
            var lblRemarks = new Label { Text = "备注:", Location = new Point(labelX, currentY + 5), AutoSize = true };
            txtRemarks = new TextBox { Location = new Point(controlX, currentY), Width = controlWidth };
            this.Controls.AddRange(new Control[] { lblRemarks, txtRemarks });
            currentY += rowHeight + 20;

            // 按钮
            btnSave = new Button 
            { 
                Text = "保存", 
                Location = new Point(controlX + 200, currentY), 
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button 
            { 
                Text = "取消", 
                Location = new Point(controlX + 310, currentY), 
                Size = new Size(100, 35),
                Font = new Font("Microsoft YaHei UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private async void LoadData()
        {
            try
            {
                // 加载分类列表
                using var context = new Aion2DbContext();
                var service = new ItemService(context);
                var categories = await service.GetAllCategoriesAsync();

                cboCategory.DataSource = categories;
                cboCategory.DisplayMember = "Name";
                cboCategory.ValueMember = "Id";

                // 如果是编辑模式，填充数据
                if (_isEditMode && _item != null)
                {
                    txtName.Text = _item.Name;
                    cboCategory.SelectedValue = _item.CategoryId;
                    nudItemLevel.Value = _item.ItemLevel ?? 0;
                    txtQuality.Text = _item.Quality ?? string.Empty;
                    nudReferencePrice.Value = _item.ReferencePrice ?? 0;
                    txtDescription.Text = _item.Description ?? string.Empty;
                    txtIconPath.Text = _item.IconPath ?? string.Empty;
                    chkIsTradable.Checked = _item.IsTradable;
                    chkIsEnabled.Checked = _item.IsEnabled;
                    nudSortOrder.Value = _item.SortOrder;
                    txtRemarks.Text = _item.Remarks ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("请输入物品名称！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (cboCategory.SelectedValue == null)
            {
                MessageBox.Show("请选择物品分类！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                return;
            }

            try
            {
                btnSave.Enabled = false;
                using var context = new Aion2DbContext();
                var service = new ItemService(context);

                // 检查名称是否已存在
                var nameExists = await service.IsItemNameExistsAsync(txtName.Text, _item?.Id);
                if (nameExists)
                {
                    MessageBox.Show("物品名称已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                Item item;
                if (_isEditMode && _item != null)
                {
                    // 更新模式
                    item = _item;
                }
                else
                {
                    // 新增模式
                    item = new Item();
                }

                // 填充数据
                item.Name = txtName.Text.Trim();
                item.CategoryId = (int)cboCategory.SelectedValue;
                item.ItemLevel = nudItemLevel.Value > 0 ? (int)nudItemLevel.Value : null;
                item.Quality = string.IsNullOrWhiteSpace(txtQuality.Text) ? null : txtQuality.Text.Trim();
                item.ReferencePrice = nudReferencePrice.Value > 0 ? nudReferencePrice.Value : null;
                item.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                item.IconPath = string.IsNullOrWhiteSpace(txtIconPath.Text) ? null : txtIconPath.Text.Trim();
                item.IsTradable = chkIsTradable.Checked;
                item.IsEnabled = chkIsEnabled.Checked;
                item.SortOrder = (int)nudSortOrder.Value;
                item.Remarks = string.IsNullOrWhiteSpace(txtRemarks.Text) ? null : txtRemarks.Text.Trim();

                bool success;
                if (_isEditMode)
                {
                    success = await service.UpdateItemAsync(item);
                }
                else
                {
                    success = await service.AddItemAsync(item);
                }

                if (success)
                {
                    MessageBox.Show("保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("保存失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

