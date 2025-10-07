using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Aion2Helper.Data;
using Aion2Helper.Services;

namespace Aion2Helper.Forms
{
    /// <summary>
    /// 价格趋势分析窗体
    /// </summary>
    public class PriceTrendAnalysisForm : Form
    {
        private Chart _priceChart;
        private GroupBox groupBoxFilter;
        private Label lblItem;
        private ComboBox comboBoxItem;
        private Label lblStartDate;
        private DateTimePicker dateTimePickerStart;
        private Label lblEndDate;
        private DateTimePicker dateTimePickerEnd;
        private Label lblStartHour;
        private NumericUpDown numericUpDownStartHour;
        private Label lblStartHourUnit;
        private Label lblEndHour;
        private NumericUpDown numericUpDownEndHour;
        private Label lblEndHourUnit;
        private Button btnAnalyze;
        private Button btnRefresh;
        private GroupBox groupBoxChart;
        private GroupBox groupBoxSummary;
        private Label lblSummary;
        private Label lblTrendResult;

        public PriceTrendAnalysisForm()
        {
            InitializeComponent();
            InitializeChart();
        }

        private void InitializeComponent()
        {
            this.Text = "价格趋势分析";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1200, 700);

            // 创建控件
            groupBoxFilter = new GroupBox();
            lblItem = new Label();
            comboBoxItem = new ComboBox();
            lblStartDate = new Label();
            dateTimePickerStart = new DateTimePicker();
            lblEndDate = new Label();
            dateTimePickerEnd = new DateTimePicker();
            lblStartHour = new Label();
            numericUpDownStartHour = new NumericUpDown();
            lblStartHourUnit = new Label();
            lblEndHour = new Label();
            numericUpDownEndHour = new NumericUpDown();
            lblEndHourUnit = new Label();
            btnAnalyze = new Button();
            btnRefresh = new Button();
            groupBoxChart = new GroupBox();
            groupBoxSummary = new GroupBox();
            lblSummary = new Label();
            lblTrendResult = new Label();

            this.SuspendLayout();

            // groupBoxFilter
            groupBoxFilter.Location = new Point(12, 12);
            groupBoxFilter.Size = new Size(1360, 100);
            groupBoxFilter.Text = "查询条件";
            groupBoxFilter.Controls.AddRange(new Control[] {
                lblItem, comboBoxItem, lblStartDate, dateTimePickerStart,
                lblEndDate, dateTimePickerEnd, lblStartHour, numericUpDownStartHour, lblStartHourUnit,
                lblEndHour, numericUpDownEndHour, lblEndHourUnit, btnAnalyze, btnRefresh
            });

            // lblItem
            lblItem.Location = new Point(15, 30);
            lblItem.Size = new Size(68, 17);
            lblItem.Text = "物品名称:";

            // comboBoxItem
            comboBoxItem.DropDownStyle = ComboBoxStyle.DropDown;
            comboBoxItem.Location = new Point(90, 27);
            comboBoxItem.Size = new Size(200, 25);
            comboBoxItem.Items.Add("全部物品");
            comboBoxItem.SelectedIndex = 0;

            // lblStartDate
            lblStartDate.Location = new Point(310, 30);
            lblStartDate.Size = new Size(68, 17);
            lblStartDate.Text = "开始日期:";

            // dateTimePickerStart
            dateTimePickerStart.Format = DateTimePickerFormat.Short;
            dateTimePickerStart.Location = new Point(385, 27);
            dateTimePickerStart.Size = new Size(120, 23);
            dateTimePickerStart.Value = DateTime.Now.AddDays(-7);

            // lblEndDate
            lblEndDate.Location = new Point(520, 30);
            lblEndDate.Size = new Size(68, 17);
            lblEndDate.Text = "结束日期:";

            // dateTimePickerEnd
            dateTimePickerEnd.Format = DateTimePickerFormat.Short;
            dateTimePickerEnd.Location = new Point(595, 27);
            dateTimePickerEnd.Size = new Size(120, 23);
            dateTimePickerEnd.Value = DateTime.Now;

            // lblStartHour
            lblStartHour.Location = new Point(15, 65);
            lblStartHour.Size = new Size(68, 17);
            lblStartHour.Text = "开始时间:";

            // numericUpDownStartHour
            numericUpDownStartHour.Location = new Point(90, 63);
            numericUpDownStartHour.Size = new Size(60, 23);
            numericUpDownStartHour.Maximum = 23;
            numericUpDownStartHour.Minimum = 0;
            numericUpDownStartHour.Value = 0;

            // lblStartHourUnit
            lblStartHourUnit.Location = new Point(155, 65);
            lblStartHourUnit.Size = new Size(20, 17);
            lblStartHourUnit.Text = "点";

            // lblEndHour
            lblEndHour.Location = new Point(180, 65);
            lblEndHour.Size = new Size(68, 17);
            lblEndHour.Text = "结束时间:";

            // numericUpDownEndHour
            numericUpDownEndHour.Location = new Point(255, 63);
            numericUpDownEndHour.Size = new Size(60, 23);
            numericUpDownEndHour.Maximum = 23;
            numericUpDownEndHour.Minimum = 0;
            numericUpDownEndHour.Value = 23;

            // lblEndHourUnit
            lblEndHourUnit.Location = new Point(320, 65);
            lblEndHourUnit.Size = new Size(20, 17);
            lblEndHourUnit.Text = "点";

            // btnAnalyze
            btnAnalyze.BackColor = Color.FromArgb(0, 123, 255);
            btnAnalyze.FlatStyle = FlatStyle.Flat;
            btnAnalyze.ForeColor = Color.White;
            btnAnalyze.Location = new Point(350, 60);
            btnAnalyze.Size = new Size(100, 30);
            btnAnalyze.Text = "📊 分析";
            btnAnalyze.UseVisualStyleBackColor = false;
            btnAnalyze.Click += BtnAnalyze_Click;

            // btnRefresh
            btnRefresh.BackColor = Color.FromArgb(40, 167, 69);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(460, 60);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Text = "🔄 刷新";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;

            // groupBoxChart
            groupBoxChart.Location = new Point(12, 120);
            groupBoxChart.Size = new Size(1360, 550);
            groupBoxChart.Text = "价格走势图";

            // groupBoxSummary
            groupBoxSummary.Location = new Point(12, 680);
            groupBoxSummary.Size = new Size(1360, 120);
            groupBoxSummary.Text = "统计摘要";
            groupBoxSummary.Controls.AddRange(new Control[] { lblSummary, lblTrendResult });

            // lblSummary
            lblSummary.AutoSize = true;
            lblSummary.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            lblSummary.ForeColor = Color.FromArgb(0, 123, 255);
            lblSummary.Location = new Point(15, 30);
            lblSummary.Text = "总记录: 0 | 平均价格: 0";

            // lblTrendResult
            lblTrendResult.AutoSize = true;
            lblTrendResult.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            lblTrendResult.ForeColor = Color.Gray;
            lblTrendResult.Location = new Point(15, 60);
            lblTrendResult.Text = "趋势: 等待分析...";

            // 添加到窗体
            this.Controls.AddRange(new Control[] {
                groupBoxFilter,
                groupBoxChart,
                groupBoxSummary
            });

            this.ResumeLayout(false);
        }

        private void InitializeChart()
        {
            try
            {
                _priceChart = new Chart();
                _priceChart.Dock = DockStyle.Fill;
                _priceChart.BackColor = Color.White;

                // 添加图表区域
                var chartArea = new ChartArea("MainArea");
                chartArea.AxisX.Title = "时间";
                chartArea.AxisX.LabelStyle.Format = "MM-dd HH:mm";
                chartArea.AxisX.LabelStyle.Angle = -45;
                chartArea.AxisY.Title = "价格";
                chartArea.AxisY.LabelStyle.Format = "N0";
                chartArea.BackColor = Color.WhiteSmoke;
                _priceChart.ChartAreas.Add(chartArea);

                // 添加图例
                var legend = new Legend("MainLegend");
                legend.Docking = Docking.Top;
                _priceChart.Legends.Add(legend);

                groupBoxChart.Controls.Add(_priceChart);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化图表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _priceChart?.Dispose();
            }
            base.Dispose(disposing);
        }

        private async void BtnAnalyze_Click(object? sender, EventArgs e)
        {
            try
            {
                btnAnalyze.Enabled = false;
                btnAnalyze.Text = "⏳ 分析中...";

                var itemName = comboBoxItem.Text == "全部物品" ? null : comboBoxItem.Text;
                var startDate = dateTimePickerStart.Value.Date;
                var endDate = dateTimePickerEnd.Value.Date.AddDays(1).AddSeconds(-1);
                var startHour = (int)numericUpDownStartHour.Value;
                var endHour = (int)numericUpDownEndHour.Value;

                using var context = new Aion2DbContext();
                var service = new PriceTrendAnalysisService(context);
                var trendData = await service.GetPriceTrendAsync(itemName, startDate, endDate, startHour, endHour);

                if (trendData.Count == 0)
                {
                    MessageBox.Show("未找到符合条件的价格数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 更新图表
                UpdateChart(trendData);

                // 获取统计摘要
                var summary = await service.GetPriceSummaryAsync(itemName, startDate, endDate, startHour, endHour);

                // 更新统计信息
                lblSummary.Text = $"总记录: {summary.TotalCount} | 平均价格: {summary.AveragePrice:N0} | 最低: {summary.MinPrice:N0} | 最高: {summary.MaxPrice:N0} | 波动: {summary.PriceRange:N0}";

                // 分析趋势
                var trend = service.AnalyzePriceTrend(trendData);
                lblTrendResult.Text = $"趋势: {trend}";

                // 根据趋势设置颜色
                if (trend.Contains("上涨"))
                    lblTrendResult.ForeColor = Color.Red;
                else if (trend.Contains("下跌"))
                    lblTrendResult.ForeColor = Color.Green;
                else
                    lblTrendResult.ForeColor = Color.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnAnalyze.Text = "📊 分析";
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                using var context = new Aion2DbContext();
                var service = new PriceTrendAnalysisService(context);
                var items = await service.GetDistinctItemNamesAsync();

                comboBoxItem.Items.Clear();
                comboBoxItem.Items.Add("全部物品");
                comboBoxItem.Items.AddRange(items.ToArray());
                comboBoxItem.SelectedIndex = 0;

                MessageBox.Show($"已加载 {items.Count} 个物品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateChart(List<PriceTrendData> trendData)
        {
            try
            {
                _priceChart.Series.Clear();

                // 平均价格系列
                var avgSeries = new Series("平均价格");
                avgSeries.ChartType = SeriesChartType.Line;
                avgSeries.BorderWidth = 3;
                avgSeries.Color = Color.FromArgb(0, 123, 255);
                avgSeries.MarkerStyle = MarkerStyle.Circle;
                avgSeries.MarkerSize = 6;

                // 最低价格系列
                var minSeries = new Series("最低价");
                minSeries.ChartType = SeriesChartType.Line;
                minSeries.BorderWidth = 2;
                minSeries.BorderDashStyle = ChartDashStyle.Dash;
                minSeries.Color = Color.FromArgb(40, 167, 69);

                // 最高价格系列
                var maxSeries = new Series("最高价");
                maxSeries.ChartType = SeriesChartType.Line;
                maxSeries.BorderWidth = 2;
                maxSeries.BorderDashStyle = ChartDashStyle.Dash;
                maxSeries.Color = Color.FromArgb(220, 53, 69);

                // 添加数据点
                foreach (var data in trendData)
                {
                    avgSeries.Points.AddXY(data.DateTime, data.AveragePrice);
                    minSeries.Points.AddXY(data.DateTime, data.MinPrice);
                    maxSeries.Points.AddXY(data.DateTime, data.MaxPrice);
                }

                _priceChart.Series.Add(avgSeries);
                _priceChart.Series.Add(minSeries);
                _priceChart.Series.Add(maxSeries);

                // 启用缩放
                _priceChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                _priceChart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
                _priceChart.ChartAreas[0].CursorX.IsUserEnabled = true;
                _priceChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                _priceChart.ChartAreas[0].CursorY.IsUserEnabled = true;
                _priceChart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新图表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
