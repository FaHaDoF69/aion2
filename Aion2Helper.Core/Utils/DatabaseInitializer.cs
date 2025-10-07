using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Config;

namespace Aion2Helper.Utils;

/// <summary>
/// 数据库初始化工具
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool Success, string Message)> InitializeAsync()
    {
        try
        {
            using var context = new Aion2DbContext();
            
            // 测试连接
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return (false, "无法连接到数据库服务器，请检查网络连接和数据库配置");
            }

            // 确保数据库存在
            var created = await context.Database.EnsureCreatedAsync();
            
            if (created)
            {
                // 数据库是新创建的，插入初始数据
                await SeedInitialDataAsync(context);
                return (true, "数据库初始化成功，已创建新数据库并插入初始数据");
            }
            else
            {
                // 数据库已存在，检查是否需要更新架构
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                    return (true, "数据库架构更新成功");
                }
                else
                {
                    return (true, "数据库连接正常，无需更新");
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"数据库初始化失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试数据库连接
    /// </summary>
    /// <returns></returns>
    public static async Task<(bool Success, string Message, TimeSpan ResponseTime)> TestConnectionAsync()
    {
        var startTime = DateTime.Now;
        try
        {
            using var context = new Aion2DbContext();
            var canConnect = await context.Database.CanConnectAsync();
            var responseTime = DateTime.Now - startTime;

            if (canConnect)
            {
                // 尝试执行一个简单查询来测试完整功能
                var count = await context.AuctionItems.CountAsync();
                return (true, $"数据库连接成功，当前拍卖行记录数: {count}", responseTime);
            }
            else
            {
                return (false, "无法连接到数据库", responseTime);
            }
        }
        catch (Exception ex)
        {
            var responseTime = DateTime.Now - startTime;
            return (false, $"数据库连接测试失败: {ex.Message}", responseTime);
        }
    }

    /// <summary>
    /// 获取数据库信息
    /// </summary>
    /// <returns></returns>
    public static async Task<DatabaseInfo> GetDatabaseInfoAsync()
    {
        try
        {
            using var context = new Aion2DbContext();
            
            var info = new DatabaseInfo
            {
                Server = DatabaseConfig.Server,
                Port = DatabaseConfig.Port,
                Database = DatabaseConfig.Database,
                Username = DatabaseConfig.Username,
                IsConnected = await context.Database.CanConnectAsync()
            };

            if (info.IsConnected)
            {
                info.AuctionItemCount = await context.AuctionItems.CountAsync();
                info.PurchaseRecordCount = await context.PurchaseRecords.CountAsync();
                info.MonitoredItemCount = await context.MonitoredItems.CountAsync();
                
                // 获取当前机器的数据统计
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                info.CurrentMachineAuctionItemCount = await context.AuctionItems
                    .CountAsync(x => x.MachineCode == currentMachineCode);
                info.CurrentMachinePurchaseRecordCount = await context.PurchaseRecords
                    .CountAsync(x => x.MachineCode == currentMachineCode);
                info.CurrentMachineMonitoredItemCount = await context.MonitoredItems
                    .CountAsync(x => x.MachineCode == currentMachineCode);

                // 获取活跃机器数量
                var activeMachines = await context.AuctionItems
                    .Where(x => x.DiscoveredAt >= DateTime.Now.AddDays(-7))
                    .Select(x => x.MachineCode)
                    .Union(context.PurchaseRecords
                        .Where(x => x.PurchaseTime >= DateTime.Now.AddDays(-7))
                        .Select(x => x.MachineCode))
                    .Distinct()
                    .CountAsync();
                
                info.ActiveMachineCount = activeMachines;
                info.CurrentMachineCode = MachineCodeHelper.GetMachineCodeDisplay();
            }

            return info;
        }
        catch (Exception ex)
        {
            return new DatabaseInfo
            {
                Server = DatabaseConfig.Server,
                Port = DatabaseConfig.Port,
                Database = DatabaseConfig.Database,
                Username = DatabaseConfig.Username,
                IsConnected = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 清理过期数据
    /// </summary>
    /// <param name="daysToKeep">保留天数</param>
    /// <returns></returns>
    public static async Task<(bool Success, string Message, int DeletedCount)> CleanupExpiredDataAsync(int daysToKeep = 30)
    {
        try
        {
            using var context = new Aion2DbContext();
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

            // 删除过期的拍卖行数据
            var expiredAuctionItems = await context.AuctionItems
                .Where(x => x.DiscoveredAt < cutoffDate)
                .ToListAsync();

            // 删除过期的购买记录（保留已完成的记录更长时间）
            var expiredPurchaseRecords = await context.PurchaseRecords
                .Where(x => x.PurchaseTime < cutoffDate && 
                           (x.Status == Models.PurchaseStatus.Failed || 
                            x.Status == Models.PurchaseStatus.Cancelled))
                .ToListAsync();

            context.AuctionItems.RemoveRange(expiredAuctionItems);
            context.PurchaseRecords.RemoveRange(expiredPurchaseRecords);

            var deletedCount = await context.SaveChangesAsync();
            
            return (true, $"清理完成，删除了 {expiredAuctionItems.Count} 条拍卖行记录和 {expiredPurchaseRecords.Count} 条购买记录", deletedCount);
        }
        catch (Exception ex)
        {
            return (false, $"数据清理失败: {ex.Message}", 0);
        }
    }

    /// <summary>
    /// 插入初始数据
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns></returns>
    private static async Task SeedInitialDataAsync(Aion2DbContext context)
    {
        // 这里可以插入一些初始数据，比如配置数据等
        // 目前暂时不需要初始数据
        await Task.CompletedTask;
    }

    /// <summary>
    /// 创建数据库备份
    /// </summary>
    /// <param name="backupPath">备份路径</param>
    /// <returns></returns>
    public static async Task<(bool Success, string Message)> CreateBackupAsync(string backupPath)
    {
        try
        {
            using var context = new Aion2DbContext();
            
            // 注意：MySQL备份通常需要使用mysqldump工具
            // 这里只是一个示例，实际实现可能需要调用外部工具
            var backupSql = $@"
                -- Aion2 数据库备份
                -- 备份时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                -- 机器码: {MachineCodeHelper.GetMachineCode()}
                
                -- 这里应该包含实际的备份SQL语句
                -- 由于Entity Framework Core的限制，建议使用mysqldump工具进行备份
            ";

            await File.WriteAllTextAsync(backupPath, backupSql);
            return (true, $"备份文件已创建: {backupPath}");
        }
        catch (Exception ex)
        {
            return (false, $"创建备份失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 数据库信息类
/// </summary>
public class DatabaseInfo
{
    /// <summary>
    /// 服务器地址
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 拍卖行物品总数
    /// </summary>
    public int AuctionItemCount { get; set; }

    /// <summary>
    /// 购买记录总数
    /// </summary>
    public int PurchaseRecordCount { get; set; }

    /// <summary>
    /// 当前机器的拍卖行物品数量
    /// </summary>
    public int CurrentMachineAuctionItemCount { get; set; }

    /// <summary>
    /// 当前机器的购买记录数量
    /// </summary>
    public int CurrentMachinePurchaseRecordCount { get; set; }

    /// <summary>
    /// 监控物品总数
    /// </summary>
    public int MonitoredItemCount { get; set; }

    /// <summary>
    /// 当前机器的监控物品数量
    /// </summary>
    public int CurrentMachineMonitoredItemCount { get; set; }

    /// <summary>
    /// 活跃机器数量（7天内有活动）
    /// </summary>
    public int ActiveMachineCount { get; set; }

    /// <summary>
    /// 当前机器码（显示用）
    /// </summary>
    public string CurrentMachineCode { get; set; } = string.Empty;

    /// <summary>
    /// 连接字符串（隐藏密码）
    /// </summary>
    public string ConnectionStringMasked => 
        $"Server={Server};Port={Port};Database={Database};User={Username};Password=***;";
}
