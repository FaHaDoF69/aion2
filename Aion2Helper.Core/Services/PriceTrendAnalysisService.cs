using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 价格趋势分析服务
    /// </summary>
    public class PriceTrendAnalysisService
    {
        private readonly Aion2DbContext _context;

        public PriceTrendAnalysisService(Aion2DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取指定日期和时间段的价格趋势数据
        /// </summary>
        /// <param name="itemName">物品名称（可选）</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="startHour">开始小时（0-23）</param>
        /// <param name="endHour">结束小时（0-23）</param>
        /// <param name="categoryId">物品分类ID（可选）</param>
        /// <returns>价格趋势数据</returns>
        public async Task<List<PriceTrendData>> GetPriceTrendAsync(
            string? itemName, 
            DateTime startDate, 
            DateTime endDate,
            int startHour = 0,
            int endHour = 23,
            int? categoryId = null)
        {
            try
            {
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                // 使用监控历史表
                var query = _context.MonitoringHistory
                    .Where(h => h.MachineCode == currentMachineCode);

                #if DEBUG
                var totalBeforeFilter = await query.CountAsync();
                Console.WriteLine($"[价格趋势] 应用分类筛选前: {totalBeforeFilter} 条记录");
                #endif

                // 筛选物品分类（通过监控物品表的物品名称匹配）
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    // 从数据库获取该分类下的所有监控物品名称
                    var itemNamesInCategory = await _context.MonitoredItems
                        .Where(m => m.CategoryId == categoryId.Value && m.MachineCode == currentMachineCode)
                        .Select(m => m.ItemName)
                        .Distinct()
                        .ToListAsync();
                    
                    #if DEBUG
                    Console.WriteLine($"[价格趋势] 分类ID={categoryId}下有 {itemNamesInCategory.Count} 个监控物品: {string.Join(", ", itemNamesInCategory)}");
                    #endif
                    
                    if (itemNamesInCategory.Count > 0)
                    {
                        query = query.Where(h => itemNamesInCategory.Contains(h.ItemName));
                        
                        #if DEBUG
                        var afterCategoryFilter = await query.CountAsync();
                        Console.WriteLine($"[价格趋势] 应用分类筛选后: {afterCategoryFilter} 条记录");
                        #endif
                    }
                    else
                    {
                        #if DEBUG
                        Console.WriteLine($"[价格趋势] 警告: 分类ID={categoryId}下没有监控物品，将返回空结果");
                        #endif
                    }
                }

                // 筛选物品名称
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    query = query.Where(h => h.ItemName.Contains(itemName));
                }

                // 筛选日期范围
                query = query.Where(h => h.DiscoveredAt >= startDate && h.DiscoveredAt <= endDate);

                // 筛选小时范围
                query = query.Where(h => h.DiscoveredAt.Hour >= startHour && h.DiscoveredAt.Hour <= endHour);

                #if DEBUG
                // 打印生成的SQL
                var sql = query.OrderBy(h => h.DiscoveredAt).ToQueryString();
                Console.WriteLine($"[价格趋势] 生成的SQL:");
                Console.WriteLine(sql);
                Console.WriteLine($"");
                #endif

                var records = await query
                    .OrderBy(h => h.DiscoveredAt)
                    .ToListAsync();

                #if DEBUG
                Console.WriteLine($"[价格趋势] 查询到 {records.Count} 条记录");
                Console.WriteLine($"  分类筛选: ID={categoryId?.ToString() ?? "全部"}");
                Console.WriteLine($"  物品筛选: {itemName ?? "全部"}");
                Console.WriteLine($"  日期范围: {startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}");
                Console.WriteLine($"  时间范围: {startHour}:00 ~ {endHour}:00");
                Console.WriteLine($"  机器码: {currentMachineCode}");
                
                if (records.Count > 0)
                {
                    Console.WriteLine($"  示例记录:");
                    foreach (var r in records.Take(3))
                    {
                        Console.WriteLine($"    - {r.ItemName} | 价格:{r.CurrentPrice:N0} | 时间:{r.DiscoveredAt:yyyy-MM-dd HH:mm}");
                    }
                }
                #endif

                // 按小时分组统计
                var trendData = records
                    .GroupBy(h => new
                    {
                        Date = h.DiscoveredAt.Date,
                        Hour = h.DiscoveredAt.Hour
                    })
                    .Select(g => new PriceTrendData
                    {
                        DateTime = g.Key.Date.AddHours(g.Key.Hour),
                        AveragePrice = g.Average(h => h.CurrentPrice),
                        MinPrice = g.Min(h => h.CurrentPrice),
                        MaxPrice = g.Max(h => h.CurrentPrice),
                        Count = g.Count(),
                        ItemName = itemName ?? "所有物品"
                    })
                    .OrderBy(t => t.DateTime)
                    .ToList();

                #if DEBUG
                Console.WriteLine($"[价格趋势] 生成了 {trendData.Count} 个趋势数据点:");
                foreach (var td in trendData)
                {
                    Console.WriteLine($"  时间:{td.DateTime:yyyy-MM-dd HH:mm} | 平均:{td.AveragePrice:N2} | 最低:{td.MinPrice:N2} | 最高:{td.MaxPrice:N2} | 数量:{td.Count}");
                }
                #endif

                return trendData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 获取价格趋势数据失败: {ex.Message}");
                return new List<PriceTrendData>();
            }
        }

        /// <summary>
        /// 获取物品列表（用于筛选）
        /// </summary>
        public async Task<List<string>> GetDistinctItemNamesAsync()
        {
            try
            {
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                // 使用监控历史表
                return await _context.MonitoringHistory
                    .Where(h => h.MachineCode == currentMachineCode)
                    .Select(h => h.ItemName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 获取物品列表失败: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取价格统计摘要
        /// </summary>
        public async Task<PriceSummary> GetPriceSummaryAsync(
            string? itemName,
            DateTime startDate,
            DateTime endDate,
            int startHour = 0,
            int endHour = 23,
            int? categoryId = null)
        {
            try
            {
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                // 使用监控历史表
                var query = _context.MonitoringHistory
                    .Where(h => h.MachineCode == currentMachineCode);

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    // 通过物品名称筛选分类
                    var itemNamesInCategory = await _context.MonitoredItems
                        .Where(m => m.CategoryId == categoryId.Value && m.MachineCode == currentMachineCode)
                        .Select(m => m.ItemName)
                        .Distinct()
                        .ToListAsync();
                    
                    if (itemNamesInCategory.Count > 0)
                    {
                        query = query.Where(h => itemNamesInCategory.Contains(h.ItemName));
                    }
                }

                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    query = query.Where(h => h.ItemName.Contains(itemName));
                }

                query = query.Where(h => h.DiscoveredAt >= startDate && h.DiscoveredAt <= endDate);
                query = query.Where(h => h.DiscoveredAt.Hour >= startHour && h.DiscoveredAt.Hour <= endHour);

                var records = await query.ToListAsync();

                if (records.Count == 0)
                {
                    return new PriceSummary();
                }

                return new PriceSummary
                {
                    ItemName = itemName ?? "所有物品",
                    TotalCount = records.Count,
                    AveragePrice = records.Average(h => h.CurrentPrice),
                    MinPrice = records.Min(h => h.CurrentPrice),
                    MaxPrice = records.Max(h => h.CurrentPrice),
                    PriceRange = records.Max(h => h.CurrentPrice) - records.Min(h => h.CurrentPrice),
                    StartDate = startDate,
                    EndDate = endDate
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 获取价格统计失败: {ex.Message}");
                return new PriceSummary();
            }
        }

        /// <summary>
        /// 计算价格变化趋势（上涨/下跌/平稳）
        /// </summary>
        public string AnalyzePriceTrend(List<PriceTrendData> trendData)
        {
            if (trendData == null || trendData.Count < 2)
                return "数据不足";

            var firstPrice = trendData.First().AveragePrice;
            var lastPrice = trendData.Last().AveragePrice;
            var priceChange = ((lastPrice - firstPrice) / firstPrice) * 100;

            if (Math.Abs(priceChange) < 5)
                return $"平稳 (变化 {priceChange:F1}%)";
            else if (priceChange > 0)
                return $"上涨 (+{priceChange:F1}%)";
            else
                return $"下跌 ({priceChange:F1}%)";
        }
    }

    /// <summary>
    /// 价格趋势数据点
    /// </summary>
    public class PriceTrendData
    {
        public DateTime DateTime { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Count { get; set; }
        public string ItemName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 价格统计摘要
    /// </summary>
    public class PriceSummary
    {
        public string ItemName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal PriceRange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
