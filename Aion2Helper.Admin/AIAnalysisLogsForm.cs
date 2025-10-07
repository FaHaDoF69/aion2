using System;
using System.Drawing;
using System.Windows.Forms;
using Aion2Helper.Models;

namespace Aion2Helper.Admin
{
    /// <summary>
    /// AI分析日志窗体（暂时提供基础框架，后续完善）
    /// </summary>
    public class AIAnalysisLogsForm : Form
    {
        private DataGridView dgvLogs;
        private Button btnRefresh;
        private ComboBox cboItemFilter;
        private ComboBox cboRecommendationFilter;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Label lblTotalCount;

        public AIAnalysisLogsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 AI分析日志";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterParent;

            // 工具栏
            var toolPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            
            var lblStartDate = new Label { Text = "开始日期:", Location = new Point(15, 25), AutoSize = true };
            dtpStartDate = new DateTimePicker { Location = new Point(90, 22), Width = 150, Value = DateTime.Now.AddDays(-7) };
            
            var lblEndDate = new Label { Text = "结束日期:", Location = new Point(260, 25), AutoSize = true };
            dtpEndDate = new DateTimePicker { Location = new Point(335, 22), Width = 150, Value = DateTime.Now };
            
            var lblItem = new Label { Text = "物品:", Location = new Point(505, 25), AutoSize = true };
            cboItemFilter = new ComboBox { Location = new Point(555, 22), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboItemFilter.Items.Add("全部物品");
            cboItemFilter.SelectedIndex = 0;
            
            btnRefresh = new Button { Text = "🔄 刷新", Location = new Point(775, 20), Size = new Size(100, 30), BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White };
            btnRefresh.Click += BtnRefresh_Click;
            
            lblTotalCount = new Label { Text = "总计: 0 条", Location = new Point(895, 25), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold) };
            
            toolPanel.Controls.AddRange(new Control[] { lblStartDate, dtpStartDate, lblEndDate, dtpEndDate, lblItem, cboItemFilter, btnRefresh, lblTotalCount });
            
            // 数据表格
            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                Font = new Font("Microsoft YaHei UI", 10F),
                RowTemplate = { Height = 32 }
            };
            
            // 启用双缓冲
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvLogs, new object[] { true });
            
            dgvLogs.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "时间", DataPropertyName = "AnalysisTime", Width = 180, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" } },
                new DataGridViewTextBoxColumn { HeaderText = "物品名称", DataPropertyName = "ItemName", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "当前价格", DataPropertyName = "CurrentPrice", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } },
                new DataGridViewTextBoxColumn { HeaderText = "AI评分", DataPropertyName = "FinalScore", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "评级", DataPropertyName = "ScoreLevel", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "AI推荐", DataPropertyName = "Recommendation", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "用户操作", DataPropertyName = "UserAction", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "实际结果", DataPropertyName = "ActualResult", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "策略", DataPropertyName = "StrategyUsed", Width = 120 }
            });
            
            this.Controls.Add(dgvLogs);
            this.Controls.Add(toolPanel);
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("AI分析日志数据加载功能开发中...\n\n当前显示的是界面框架，后续将实现：\n• 从数据库加载AI分析记录\n• 筛选和搜索\n• 查看详细分析报告\n• 导出分析结果", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}


