using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services;

/// <summary>
/// 数据库服务类
/// </summary>
public class DatabaseService : IDisposable
{
    private readonly Aion2DbContext _context;
    private readonly string _currentMachineCode;
    private bool _disposed = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    public DatabaseService()
    {
        _context = new Aion2DbContext();
        _currentMachineCode = MachineCodeHelper.GetMachineCode();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public DatabaseService(Aion2DbContext context)
    {
        _context = context;
        _currentMachineCode = MachineCodeHelper.GetMachineCode();
    }

    /// <summary>
    /// 获取当前机器码
    /// </summary>
    public string CurrentMachineCode => _currentMachineCode;

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <returns></returns>
    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
            return true;
        }
        catch (Exception ex)
        {
            // 记录错误日志
            #if DEBUG
            Aion2Helper.Program.LogError("数据库", $"初始化失败: {ex.Message}");
            #endif
            return false;
        }
    }

    /// <summary>
    /// 测试数据库连接
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    #region AuctionItem 相关方法

    /// <summary>
    /// 添加拍卖行物品
    /// </summary>
    /// <param name="item">拍卖行物品</param>
    /// <returns></returns>
    public async Task<AuctionItem> AddAuctionItemAsync(AuctionItem item)
    {
        item.MachineCode = _currentMachineCode;
        item.DiscoveredAt = DateTime.Now;
        
        _context.AuctionItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// 批量添加拍卖行物品
    /// </summary>
    /// <param name="items">拍卖行物品列表</param>
    /// <returns></returns>
    public async Task<int> AddAuctionItemsAsync(IEnumerable<AuctionItem> items)
    {
        var itemList = items.ToList();
        foreach (var item in itemList)
        {
            item.MachineCode = _currentMachineCode;
            item.DiscoveredAt = DateTime.Now;
        }

        _context.AuctionItems.AddRange(itemList);
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 获取当前机器的拍卖行物品
    /// </summary>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<AuctionItem>> GetCurrentMachineAuctionItemsAsync(int limit = 100)
    {
        return await _context.AuctionItems
            .Where(x => x.MachineCode == _currentMachineCode)
            .OrderByDescending(x => x.DiscoveredAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定机器的拍卖行物品
    /// </summary>
    /// <param name="machineCode">机器码</param>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<AuctionItem>> GetAuctionItemsByMachineAsync(string machineCode, int limit = 100)
    {
        return await _context.AuctionItems
            .Where(x => x.MachineCode == machineCode)
            .OrderByDescending(x => x.DiscoveredAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 按物品名称搜索当前机器的拍卖行物品
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<AuctionItem>> SearchCurrentMachineAuctionItemsAsync(string itemName, int limit = 50)
    {
        return await _context.AuctionItems
            .Where(x => x.MachineCode == _currentMachineCode && x.Name.Contains(itemName))
            .OrderByDescending(x => x.DiscoveredAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 获取当前机器的异常低价物品
    /// </summary>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<AuctionItem>> GetCurrentMachineAbnormalPriceItemsAsync(int limit = 50)
    {
        return await _context.AuctionItems
            .Where(x => x.MachineCode == _currentMachineCode && x.IsAbnormalPrice)
            .OrderByDescending(x => x.DiscoveredAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 清理当前机器的过期拍卖行数据
    /// </summary>
    /// <param name="daysToKeep">保留天数</param>
    /// <returns></returns>
    public async Task<int> CleanupCurrentMachineAuctionItemsAsync(int daysToKeep = 7)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        var itemsToDelete = await _context.AuctionItems
            .Where(x => x.MachineCode == _currentMachineCode && x.DiscoveredAt < cutoffDate)
            .ToListAsync();

        _context.AuctionItems.RemoveRange(itemsToDelete);
        return await _context.SaveChangesAsync();
    }

    #endregion

    #region PurchaseRecord 相关方法

    /// <summary>
    /// 添加购买记录
    /// </summary>
    /// <param name="record">购买记录</param>
    /// <returns></returns>
    public async Task<PurchaseRecord> AddPurchaseRecordAsync(PurchaseRecord record)
    {
        record.MachineCode = _currentMachineCode;
        record.PurchaseTime = DateTime.Now;
        
        _context.PurchaseRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    /// <summary>
    /// 更新购买记录
    /// </summary>
    /// <param name="record">购买记录</param>
    /// <returns></returns>
    public async Task<PurchaseRecord> UpdatePurchaseRecordAsync(PurchaseRecord record)
    {
        _context.PurchaseRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }

    /// <summary>
    /// 获取当前机器的购买记录
    /// </summary>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<PurchaseRecord>> GetCurrentMachinePurchaseRecordsAsync(int limit = 100)
    {
        return await _context.PurchaseRecords
            .Where(x => x.MachineCode == _currentMachineCode)
            .OrderByDescending(x => x.PurchaseTime)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 按状态获取当前机器的购买记录
    /// </summary>
    /// <param name="status">购买状态</param>
    /// <param name="limit">限制数量</param>
    /// <returns></returns>
    public async Task<List<PurchaseRecord>> GetCurrentMachinePurchaseRecordsByStatusAsync(PurchaseStatus status, int limit = 50)
    {
        return await _context.PurchaseRecords
            .Where(x => x.MachineCode == _currentMachineCode && x.Status == status)
            .OrderByDescending(x => x.PurchaseTime)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 获取当前机器的购买统计
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns></returns>
    public async Task<(int TotalCount, decimal TotalAmount, decimal TotalProfit)> GetCurrentMachinePurchaseStatsAsync(int days = 7)
    {
        var startDate = DateTime.Now.AddDays(-days);
        var records = await _context.PurchaseRecords
            .Where(x => x.MachineCode == _currentMachineCode && 
                       x.PurchaseTime >= startDate && 
                       x.Status == PurchaseStatus.Completed)
            .ToListAsync();

        var totalCount = records.Count;
        var totalAmount = records.Sum(x => x.TotalAmount);
        var totalProfit = records.Sum(x => x.ActualProfit ?? x.ExpectedProfit);

        return (totalCount, totalAmount, totalProfit);
    }

    #endregion

    #region MonitoredItem 相关方法

    /// <summary>
    /// 添加监控物品
    /// </summary>
    /// <param name="item">监控物品</param>
    /// <returns></returns>
    public async Task<MonitoredItem> AddMonitoredItemAsync(MonitoredItem item)
    {
        item.MachineCode = _currentMachineCode;
        item.CreatedAt = DateTime.Now;
        item.UpdatedAt = DateTime.Now;
        
        _context.MonitoredItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// 获取当前机器的监控物品
    /// </summary>
    /// <param name="enabledOnly">是否只获取启用的物品</param>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> GetCurrentMachineMonitoredItemsAsync(bool enabledOnly = true)
    {
        var query = _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode);

        if (enabledOnly)
        {
            query = query.Where(x => x.IsEnabled);
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }

    /// <summary>
    /// 根据物品名称查找监控物品
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <returns></returns>
    public async Task<MonitoredItem?> FindMonitoredItemByNameAsync(string itemName)
    {
        return await _context.MonitoredItems
            .FirstOrDefaultAsync(x => x.MachineCode == _currentMachineCode && x.ItemName == itemName);
    }

    /// <summary>
    /// 更新监控物品的发现信息
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="foundPrice">发现的价格</param>
    /// <returns></returns>
    public async Task<bool> UpdateMonitoredItemFoundInfoAsync(string itemName, decimal foundPrice)
    {
        var item = await FindMonitoredItemByNameAsync(itemName);
        if (item == null)
            return false;

        item.LastFoundAt = DateTime.Now;
        item.LastFoundPrice = foundPrice;
        item.TotalFoundCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region 机器管理相关方法

    /// <summary>
    /// 获取所有活跃的机器码
    /// </summary>
    /// <param name="days">活跃天数</param>
    /// <returns></returns>
    public async Task<List<string>> GetActiveMachineCodesAsync(int days = 7)
    {
        var startDate = DateTime.Now.AddDays(-days);
        
        var auctionMachines = await _context.AuctionItems
            .Where(x => x.DiscoveredAt >= startDate)
            .Select(x => x.MachineCode)
            .Distinct()
            .ToListAsync();

        var purchaseMachines = await _context.PurchaseRecords
            .Where(x => x.PurchaseTime >= startDate)
            .Select(x => x.MachineCode)
            .Distinct()
            .ToListAsync();

        return auctionMachines.Union(purchaseMachines).Distinct().ToList();
    }

    /// <summary>
    /// 获取机器活动统计
    /// </summary>
    /// <param name="machineCode">机器码</param>
    /// <param name="days">统计天数</param>
    /// <returns></returns>
    public async Task<(int AuctionItemCount, int PurchaseRecordCount, DateTime? LastActivity)> GetMachineActivityStatsAsync(string machineCode, int days = 7)
    {
        var startDate = DateTime.Now.AddDays(-days);

        var auctionCount = await _context.AuctionItems
            .CountAsync(x => x.MachineCode == machineCode && x.DiscoveredAt >= startDate);

        var purchaseCount = await _context.PurchaseRecords
            .CountAsync(x => x.MachineCode == machineCode && x.PurchaseTime >= startDate);

        var lastAuctionActivity = await _context.AuctionItems
            .Where(x => x.MachineCode == machineCode)
            .MaxAsync(x => (DateTime?)x.DiscoveredAt);

        var lastPurchaseActivity = await _context.PurchaseRecords
            .Where(x => x.MachineCode == machineCode)
            .MaxAsync(x => (DateTime?)x.PurchaseTime);

        var lastActivity = new[] { lastAuctionActivity, lastPurchaseActivity }
            .Where(x => x.HasValue)
            .Max();

        return (auctionCount, purchaseCount, lastActivity);
    }

    #endregion

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
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }
    }
}
