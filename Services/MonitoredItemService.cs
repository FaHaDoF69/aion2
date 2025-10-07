using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services;

/// <summary>
/// 监控物品服务类
/// </summary>
public class MonitoredItemService : IDisposable
{
    private readonly Aion2DbContext _context;
    private readonly string _currentMachineCode;
    private bool _disposed = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MonitoredItemService()
    {
        _context = new Aion2DbContext();
        _currentMachineCode = MachineCodeHelper.GetMachineCode();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public MonitoredItemService(Aion2DbContext context)
    {
        _context = context;
        _currentMachineCode = MachineCodeHelper.GetMachineCode();
    }

    /// <summary>
    /// 获取当前机器码
    /// </summary>
    public string CurrentMachineCode => _currentMachineCode;

    #region 基础CRUD操作

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
    /// 批量添加监控物品
    /// </summary>
    /// <param name="items">监控物品列表</param>
    /// <returns></returns>
    public async Task<int> AddMonitoredItemsAsync(IEnumerable<MonitoredItem> items)
    {
        var itemList = items.ToList();
        foreach (var item in itemList)
        {
            item.MachineCode = _currentMachineCode;
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
        }

        _context.MonitoredItems.AddRange(itemList);
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 更新监控物品
    /// </summary>
    /// <param name="item">监控物品</param>
    /// <returns></returns>
    public async Task<MonitoredItem> UpdateMonitoredItemAsync(MonitoredItem item)
    {
        item.UpdatedAt = DateTime.Now;
        _context.MonitoredItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// 删除监控物品
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <returns></returns>
    public async Task<bool> DeleteMonitoredItemAsync(long id)
    {
        var item = await _context.MonitoredItems
            .FirstOrDefaultAsync(x => x.Id == id && x.MachineCode == _currentMachineCode);
        
        if (item == null)
            return false;

        _context.MonitoredItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 根据ID获取监控物品
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <returns></returns>
    public async Task<MonitoredItem?> GetMonitoredItemByIdAsync(long id)
    {
        return await _context.MonitoredItems
            .FirstOrDefaultAsync(x => x.Id == id && x.MachineCode == _currentMachineCode);
    }

    #endregion

    #region 查询操作

    /// <summary>
    /// 获取当前机器的所有监控物品
    /// </summary>
    /// <param name="includeDisabled">是否包含禁用的物品</param>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> GetCurrentMachineMonitoredItemsAsync(bool includeDisabled = false)
    {
        try
        {
            // 调试信息：显示查询参数
            #if DEBUG
            Aion2Helper.Program.LogInfo("数据库", $"查询监控物品 - 机器码: {_currentMachineCode}, 包含禁用: {includeDisabled}");
            #endif
            
            var query = _context.MonitoredItems
                .Where(x => x.MachineCode == _currentMachineCode);

            if (!includeDisabled)
            {
                query = query.Where(x => x.IsEnabled);
            }

            var result = await query
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.ItemName)
                .ToListAsync();
            
            #if DEBUG
            Aion2Helper.Program.LogSuccess("数据库", $"查询结果: {result.Count} 个物品");
            #endif
            return result;
        }
        catch (Exception ex)
        {
            #if DEBUG
            Aion2Helper.Program.LogError("数据库", $"查询监控物品异常: {ex.Message}");
            #endif
            throw;
        }
    }

    /// <summary>
    /// 获取启用的监控物品（按优先级排序）
    /// </summary>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> GetEnabledMonitoredItemsAsync()
    {
        return await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && x.IsEnabled)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }

    /// <summary>
    /// 获取启用自动购买的监控物品
    /// </summary>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> GetAutoPurchaseEnabledItemsAsync()
    {
        return await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && 
                       x.IsEnabled && 
                       x.AutoPurchaseEnabled)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }

    /// <summary>
    /// 按物品名称搜索监控物品
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="exactMatch">是否精确匹配</param>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> SearchMonitoredItemsByNameAsync(string itemName, bool exactMatch = false)
    {
        var query = _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode);

        if (exactMatch)
        {
            query = query.Where(x => x.ItemName == itemName);
        }
        else
        {
            query = query.Where(x => x.ItemName.Contains(itemName));
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }

    /// <summary>
    /// 按类别获取监控物品
    /// </summary>
    /// <param name="category">物品类别</param>
    /// <returns></returns>
    public async Task<List<MonitoredItem>> GetMonitoredItemsByCategoryAsync(string category)
    {
        return await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && x.Category == category)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.ItemName)
            .ToListAsync();
    }


    /// <summary>
    /// 获取所有物品类别
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        return await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && !string.IsNullOrEmpty(x.Category))
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    #endregion

    #region 监控状态更新

    /// <summary>
    /// 更新物品的最后监控时间
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> UpdateLastMonitoredTimeAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.LastMonitoredAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新物品的发现信息
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="foundPrice">发现的价格</param>
    /// <returns></returns>
    public async Task<bool> UpdateItemFoundInfoAsync(long itemId, decimal foundPrice)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.LastFoundAt = DateTime.Now;
        item.LastFoundPrice = foundPrice;
        item.TotalFoundCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新物品的购买计数
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> UpdatePurchaseCountAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.TotalPurchaseCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 批量更新监控时间
    /// </summary>
    /// <param name="itemIds">物品ID列表</param>
    /// <returns></returns>
    public async Task<int> BatchUpdateMonitoredTimeAsync(IEnumerable<long> itemIds)
    {
        var items = await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && itemIds.Contains(x.Id))
            .ToListAsync();

        var now = DateTime.Now;
        foreach (var item in items)
        {
            item.LastMonitoredAt = now;
        }

        return await _context.SaveChangesAsync();
    }

    #endregion

    #region 启用/禁用操作

    /// <summary>
    /// 启用监控物品
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> EnableMonitoredItemAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.IsEnabled = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 禁用监控物品
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> DisableMonitoredItemAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.IsEnabled = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 启用自动购买
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> EnableAutoPurchaseAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.AutoPurchaseEnabled = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 禁用自动购买
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns></returns>
    public async Task<bool> DisableAutoPurchaseAsync(long itemId)
    {
        var item = await GetMonitoredItemByIdAsync(itemId);
        if (item == null)
            return false;

        item.AutoPurchaseEnabled = false;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region 统计信息

    /// <summary>
    /// 获取监控物品统计信息
    /// </summary>
    /// <returns></returns>
    public async Task<MonitoredItemStats> GetMonitoredItemStatsAsync()
    {
        var items = await _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode)
            .ToListAsync();

        return new MonitoredItemStats
        {
            TotalCount = items.Count,
            EnabledCount = items.Count(x => x.IsEnabled),
            DisabledCount = items.Count(x => !x.IsEnabled),
            AutoPurchaseEnabledCount = items.Count(x => x.AutoPurchaseEnabled),
            TotalFoundCount = items.Sum(x => x.TotalFoundCount),
            TotalPurchaseCount = items.Sum(x => x.TotalPurchaseCount),
            CategoryCount = items.Where(x => !string.IsNullOrEmpty(x.Category))
                               .Select(x => x.Category)
                               .Distinct()
                               .Count(),
            HighPriorityCount = items.Count(x => x.Priority >= 8),
            MediumPriorityCount = items.Count(x => x.Priority >= 5 && x.Priority < 8),
            LowPriorityCount = items.Count(x => x.Priority < 5)
        };
    }

    /// <summary>
    /// 检查物品名称是否已存在
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="excludeId">排除的ID（用于更新时检查）</param>
    /// <returns></returns>
    public async Task<bool> IsItemNameExistsAsync(string itemName, long? excludeId = null)
    {
        var query = _context.MonitoredItems
            .Where(x => x.MachineCode == _currentMachineCode && x.ItemName == itemName);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
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

/// <summary>
/// 监控物品统计信息
/// </summary>
public class MonitoredItemStats
{
    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 启用数量
    /// </summary>
    public int EnabledCount { get; set; }

    /// <summary>
    /// 禁用数量
    /// </summary>
    public int DisabledCount { get; set; }

    /// <summary>
    /// 启用自动购买数量
    /// </summary>
    public int AutoPurchaseEnabledCount { get; set; }

    /// <summary>
    /// 总发现次数
    /// </summary>
    public int TotalFoundCount { get; set; }

    /// <summary>
    /// 总购买次数
    /// </summary>
    public int TotalPurchaseCount { get; set; }

    /// <summary>
    /// 类别数量
    /// </summary>
    public int CategoryCount { get; set; }

    /// <summary>
    /// 高优先级数量
    /// </summary>
    public int HighPriorityCount { get; set; }

    /// <summary>
    /// 中等优先级数量
    /// </summary>
    public int MediumPriorityCount { get; set; }

    /// <summary>
    /// 低优先级数量
    /// </summary>
    public int LowPriorityCount { get; set; }

    /// <summary>
    /// 发现成功率
    /// </summary>
    public double FoundSuccessRate => TotalCount > 0 ? (double)TotalFoundCount / TotalCount : 0;

    /// <summary>
    /// 购买成功率
    /// </summary>
    public double PurchaseSuccessRate => TotalFoundCount > 0 ? (double)TotalPurchaseCount / TotalFoundCount : 0;
}
