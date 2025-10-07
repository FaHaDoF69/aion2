using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services;

/// <summary>
/// 监控历史服务类
/// </summary>
public class MonitoringHistoryService : IDisposable
{
    private readonly Aion2DbContext _context;
    private bool _disposed = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MonitoringHistoryService()
    {
        _context = new Aion2DbContext();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public MonitoringHistoryService(Aion2DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 添加监控历史记录
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="currentPrice">当前价格</param>
    /// <param name="expectedPrice">预期价格</param>
    /// <param name="strategy">策略</param>
    /// <param name="riskLevel">风险等级</param>
    /// <param name="itemLevel">物品等级</param>
    /// <param name="sellerName">卖家名称</param>
    /// <param name="quantity">数量</param>
    /// <returns>添加的记录</returns>
    public async Task<MonitoringHistory> AddMonitoringRecordAsync(
        string itemName,
        decimal currentPrice,
        decimal expectedPrice,
        string strategy,
        RiskLevel riskLevel,
        int? itemLevel = null,
        string? sellerName = null,
        int quantity = 1)
    {
        try
        {
            var expectedProfit = expectedPrice - currentPrice;
            var profitRate = currentPrice > 0 ? expectedProfit / currentPrice : 0;
            var priceDeviation = expectedPrice > 0 ? (currentPrice - expectedPrice) / expectedPrice : 0;

            var record = new MonitoringHistory
            {
                MachineCode = MachineCodeHelper.GetMachineCode(),
                ItemName = itemName,
                ItemLevel = itemLevel,
                CurrentPrice = currentPrice,
                ExpectedPrice = expectedPrice,
                ExpectedProfit = expectedProfit,
                ProfitRate = profitRate,
                Strategy = strategy,
                RiskLevel = riskLevel,
                SellerName = sellerName,
                Quantity = quantity,
                PriceDeviation = priceDeviation,
                IsAbnormalPrice = Math.Abs(priceDeviation) > 0.5m, // 价格偏差超过50%认为异常
                DiscoveredAt = DateTime.Now
            };

            _context.MonitoringHistory.Add(record);
            await _context.SaveChangesAsync();

            #if DEBUG
            Logger.LogInfo("监控历史", $"添加记录: {itemName} @ {currentPrice:N0}");
            #endif

            return record;
        }
        catch (Exception ex)
        {
            #if DEBUG
            Logger.LogError("监控历史", $"添加记录失败: {ex.Message}");
            #endif
            
            throw;
        }
    }

    /// <summary>
    /// 分页查询监控历史记录
    /// </summary>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <param name="processStatus">处理状态筛选（可选）</param>
    /// <param name="itemName">物品名称筛选（可选）</param>
    /// <param name="isProcessed">是否已处理筛选（可选）</param>
    /// <returns>分页结果</returns>
    public async Task<PaginatedResult<MonitoringHistory>> GetPaginatedHistoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        MonitoringProcessStatus? processStatus = null,
        string? itemName = null,
        bool? isProcessed = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            // 构建查询
            var query = _context.MonitoringHistory
                .Where(x => x.MachineCode == currentMachineCode);

            // 应用筛选条件
            if (startDate.HasValue)
            {
                query = query.Where(x => x.DiscoveredAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.DiscoveredAt <= endOfDay);
            }

            if (processStatus.HasValue)
            {
                query = query.Where(x => x.ProcessStatus == processStatus.Value);
            }

            if (isProcessed.HasValue)
            {
                query = query.Where(x => x.IsProcessed == isProcessed.Value);
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                query = query.Where(x => x.ItemName.Contains(itemName));
            }

            // 按发现时间倒序排列
            query = query.OrderByDescending(x => x.DiscoveredAt);

            // 获取总记录数
            var totalCount = await query.CountAsync();

            // 计算分页信息
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages == 0 ? 1 : totalPages));

            // 获取当前页数据
            var records = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<MonitoringHistory>
            {
                Items = records,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }
        catch (Exception ex)
        {
            #if DEBUG
            Logger.LogError("监控历史", $"分页查询失败: {ex.Message}");
            #endif
            
            // 返回空结果
            return new PaginatedResult<MonitoringHistory>
            {
                Items = new List<MonitoringHistory>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = pageSize,
                TotalPages = 0,
                HasPreviousPage = false,
                HasNextPage = false
            };
        }
    }

    /// <summary>
    /// 获取监控历史统计信息
    /// </summary>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <returns>统计信息</returns>
    public async Task<MonitoringHistoryStats> GetHistoryStatsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            // 构建查询
            var query = _context.MonitoringHistory
                .Where(x => x.MachineCode == currentMachineCode);

            // 应用日期筛选
            if (startDate.HasValue)
            {
                query = query.Where(x => x.DiscoveredAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.DiscoveredAt <= endOfDay);
            }

            // 获取统计数据
            var totalCount = await query.CountAsync();
            var purchasedCount = await query.CountAsync(x => x.ProcessStatus == MonitoringProcessStatus.Purchased);
            var ignoredCount = await query.CountAsync(x => x.ProcessStatus == MonitoringProcessStatus.Ignored);
            var pendingCount = await query.CountAsync(x => x.ProcessStatus == MonitoringProcessStatus.Pending);
            var abnormalPriceCount = await query.CountAsync(x => x.IsAbnormalPrice);
            
            var totalExpectedProfit = await query.SumAsync(x => (decimal?)x.ExpectedProfit) ?? 0;
            var averageProfitRate = totalCount > 0 ? await query.AverageAsync(x => (decimal?)x.ProfitRate) ?? 0 : 0;

            return new MonitoringHistoryStats
            {
                TotalCount = totalCount,
                PurchasedCount = purchasedCount,
                IgnoredCount = ignoredCount,
                PendingCount = pendingCount,
                AbnormalPriceCount = abnormalPriceCount,
                TotalExpectedProfit = totalExpectedProfit,
                AverageProfitRate = averageProfitRate,
                ProcessRate = totalCount > 0 ? (double)(purchasedCount + ignoredCount) / totalCount : 0
            };
        }
        catch (Exception ex)
        {
            #if DEBUG
            Logger.LogError("监控历史", $"获取统计信息失败: {ex.Message}");
            #endif
            
            return new MonitoringHistoryStats();
        }
    }

    /// <summary>
    /// 更新监控记录处理状态
    /// </summary>
    /// <param name="recordId">记录ID</param>
    /// <param name="processStatus">处理状态</param>
    /// <param name="purchaseRecordId">关联的购买记录ID（可选）</param>
    /// <param name="notes">备注（可选）</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateProcessStatusAsync(long recordId, MonitoringProcessStatus processStatus, string? notes = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            var record = await _context.MonitoringHistory
                .FirstOrDefaultAsync(x => x.Id == recordId && x.MachineCode == currentMachineCode);

            if (record == null)
            {
                return false;
            }

            record.ProcessStatus = processStatus;
            record.IsProcessed = processStatus != MonitoringProcessStatus.Pending;
            record.ProcessedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(notes))
            {
                record.Notes = notes;
            }

            await _context.SaveChangesAsync();

            #if DEBUG
            Logger.LogInfo("监控历史", $"更新处理状态: {record.ItemName} -> {processStatus}");
            #endif

            return true;
        }
        catch (Exception ex)
        {
            #if DEBUG
            Logger.LogError("监控历史", $"更新处理状态失败: {ex.Message}");
            #endif
            
            return false;
        }
    }

    /// <summary>
    /// 获取最近的监控记录
    /// </summary>
    /// <param name="count">记录数量</param>
    /// <returns>最近的监控记录</returns>
    public async Task<List<MonitoringHistory>> GetRecentMonitoringRecordsAsync(int count = 10)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            return await _context.MonitoringHistory
                .Where(x => x.MachineCode == currentMachineCode)
                .OrderByDescending(x => x.DiscoveredAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            #if DEBUG
            Logger.LogError("监控历史", $"获取最近监控记录失败: {ex.Message}");
            #endif
            
            return new List<MonitoringHistory>();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// 监控历史统计信息
/// </summary>
public class MonitoringHistoryStats
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已购买数量
    /// </summary>
    public int PurchasedCount { get; set; }

    /// <summary>
    /// 已忽略数量
    /// </summary>
    public int IgnoredCount { get; set; }

    /// <summary>
    /// 待处理数量
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 异常价格数量
    /// </summary>
    public int AbnormalPriceCount { get; set; }

    /// <summary>
    /// 总预期利润
    /// </summary>
    public decimal TotalExpectedProfit { get; set; }

    /// <summary>
    /// 平均利润率
    /// </summary>
    public decimal AverageProfitRate { get; set; }

    /// <summary>
    /// 处理率
    /// </summary>
    public double ProcessRate { get; set; }
}
