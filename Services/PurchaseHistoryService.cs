using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services;

/// <summary>
/// 交易历史服务类
/// </summary>
public class PurchaseHistoryService : IDisposable
{
    private readonly Aion2DbContext _context;
    private bool _disposed = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PurchaseHistoryService()
    {
        _context = new Aion2DbContext();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public PurchaseHistoryService(Aion2DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 分页查询交易历史记录
    /// </summary>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <param name="status">状态筛选（可选）</param>
    /// <param name="itemName">物品名称筛选（可选）</param>
    /// <returns>分页结果</returns>
    public async Task<PaginatedResult<PurchaseRecord>> GetPaginatedHistoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        PurchaseStatus? status = null,
        string? itemName = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            // 构建查询
            var query = _context.PurchaseRecords
                .Where(x => x.MachineCode == currentMachineCode);

            // 应用筛选条件
            if (startDate.HasValue)
            {
                query = query.Where(x => x.PurchaseTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // 结束日期包含当天的23:59:59
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.PurchaseTime <= endOfDay);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                query = query.Where(x => x.ItemName.Contains(itemName));
            }

            // 按购买时间倒序排列
            query = query.OrderByDescending(x => x.PurchaseTime);

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

            return new PaginatedResult<PurchaseRecord>
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
            Aion2Helper.Program.LogError("交易历史", $"分页查询失败: {ex.Message}");
            #endif
            
            // 返回空结果
            return new PaginatedResult<PurchaseRecord>
            {
                Items = new List<PurchaseRecord>(),
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
    /// 获取交易历史统计信息
    /// </summary>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <returns>统计信息</returns>
    public async Task<PurchaseHistoryStats> GetHistoryStatsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            // 构建查询
            var query = _context.PurchaseRecords
                .Where(x => x.MachineCode == currentMachineCode);

            // 应用日期筛选
            if (startDate.HasValue)
            {
                query = query.Where(x => x.PurchaseTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.PurchaseTime <= endOfDay);
            }

            // 获取统计数据
            var totalCount = await query.CountAsync();
            var completedCount = await query.CountAsync(x => x.Status == PurchaseStatus.Completed);
            var failedCount = await query.CountAsync(x => x.Status == PurchaseStatus.Failed);
            var cancelledCount = await query.CountAsync(x => x.Status == PurchaseStatus.Cancelled);
            
            var totalAmount = await query.SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
            var totalProfit = await query.SumAsync(x => x.ActualProfit) ?? 0;
            var expectedProfit = await query.SumAsync(x => (decimal?)x.ExpectedProfit) ?? 0;

            return new PurchaseHistoryStats
            {
                TotalCount = totalCount,
                CompletedCount = completedCount,
                FailedCount = failedCount,
                CancelledCount = cancelledCount,
                PendingCount = totalCount - completedCount - failedCount - cancelledCount,
                TotalAmount = totalAmount,
                TotalProfit = totalProfit,
                ExpectedProfit = expectedProfit,
                SuccessRate = totalCount > 0 ? (double)completedCount / totalCount : 0
            };
        }
        catch (Exception ex)
        {
            #if DEBUG
            Aion2Helper.Program.LogError("交易历史", $"获取统计信息失败: {ex.Message}");
            #endif
            
            return new PurchaseHistoryStats();
        }
    }

    /// <summary>
    /// 获取最近的交易记录
    /// </summary>
    /// <param name="count">记录数量</param>
    /// <returns>最近的交易记录</returns>
    public async Task<List<PurchaseRecord>> GetRecentPurchasesAsync(int count = 10)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            return await _context.PurchaseRecords
                .Where(x => x.MachineCode == currentMachineCode)
                .OrderByDescending(x => x.PurchaseTime)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            #if DEBUG
            Aion2Helper.Program.LogError("交易历史", $"获取最近交易记录失败: {ex.Message}");
            #endif
            
            return new List<PurchaseRecord>();
        }
    }

    /// <summary>
    /// 添加交易记录
    /// </summary>
    /// <param name="record">交易记录</param>
    /// <returns>添加的记录</returns>
    public async Task<PurchaseRecord> AddPurchaseRecordAsync(PurchaseRecord record)
    {
        try
        {
            // 确保机器码正确
            record.MachineCode = MachineCodeHelper.GetMachineCode();
            record.PurchaseTime = DateTime.Now;

            _context.PurchaseRecords.Add(record);
            await _context.SaveChangesAsync();

            #if DEBUG
            Aion2Helper.Program.LogSuccess("交易历史", $"添加交易记录: {record.ItemName} x{record.Quantity} @ {record.Price:N0}");
            #endif

            return record;
        }
        catch (Exception ex)
        {
            #if DEBUG
            Aion2Helper.Program.LogError("交易历史", $"添加交易记录失败: {ex.Message}");
            #endif
            
            throw;
        }
    }

    /// <summary>
    /// 更新交易记录状态
    /// </summary>
    /// <param name="recordId">记录ID</param>
    /// <param name="status">新状态</param>
    /// <param name="actualProfit">实际利润（可选）</param>
    /// <param name="notes">备注（可选）</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdatePurchaseStatusAsync(long recordId, PurchaseStatus status, decimal? actualProfit = null, string? notes = null)
    {
        try
        {
            var currentMachineCode = MachineCodeHelper.GetMachineCode();
            
            var record = await _context.PurchaseRecords
                .FirstOrDefaultAsync(x => x.Id == recordId && x.MachineCode == currentMachineCode);

            if (record == null)
            {
                return false;
            }

            record.Status = status;
            
            if (actualProfit.HasValue)
            {
                record.ActualProfit = actualProfit.Value;
            }

            if (!string.IsNullOrWhiteSpace(notes))
            {
                record.Notes = notes;
            }

            await _context.SaveChangesAsync();

            #if DEBUG
            Aion2Helper.Program.LogInfo("交易历史", $"更新交易状态: {record.ItemName} -> {status}");
            #endif

            return true;
        }
        catch (Exception ex)
        {
            #if DEBUG
            Aion2Helper.Program.LogError("交易历史", $"更新交易状态失败: {ex.Message}");
            #endif
            
            return false;
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
/// 分页结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// 数据项
    /// </summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每页记录数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage { get; set; }
}

/// <summary>
/// 交易历史统计信息
/// </summary>
public class PurchaseHistoryStats
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已完成数量
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 已取消数量
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// 待处理数量
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 总利润
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// 预期利润
    /// </summary>
    public decimal ExpectedProfit { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }
}
