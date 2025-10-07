using System;
using System.Threading.Tasks;
using MySqlConnector;
using Aion2Helper.Config;
using Aion2Helper.Utils;

namespace Aion2Helper.Utils;

/// <summary>
/// 数据库连接检查器
/// </summary>
public static class DatabaseConnectionChecker
{
    /// <summary>
    /// 检查数据库连接状态
    /// </summary>
    /// <returns>连接结果</returns>
    public static async Task<DatabaseConnectionResult> CheckConnectionAsync()
    {
        var result = new DatabaseConnectionResult
        {
            MachineCode = MachineCodeHelper.GetMachineCode(), // 使用完整机器码
            StartTime = DateTime.Now
        };

        try
        {
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            await connection.OpenAsync();
            
            result.IsConnected = true;
            result.ServerVersion = connection.ServerVersion;
            result.ConnectionId = (uint)connection.ServerThread;
            
            // 测试简单查询
            using var command = new MySqlCommand("SELECT NOW() as server_time", connection);
            var serverTime = await command.ExecuteScalarAsync();
            result.ServerTime = Convert.ToDateTime(serverTime);
            
            // 检查数据库是否存在
            using var dbCommand = new MySqlCommand(
                $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{DatabaseConfig.Database}'", 
                connection);
            var dbExists = await dbCommand.ExecuteScalarAsync();
            result.DatabaseExists = dbExists != null;
            
            if (result.DatabaseExists)
            {
                // 检查表是否存在
                using var tableCommand = new MySqlCommand(
                    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{DatabaseConfig.Database}' AND TABLE_NAME IN ('auction_items', 'purchase_records', 'monitored_items')", 
                    connection);
                var tableCount = Convert.ToInt32(await tableCommand.ExecuteScalarAsync());
                result.TablesExist = tableCount == 3;
            }
            
            result.ResponseTime = DateTime.Now - result.StartTime;
            result.Message = "数据库连接成功";
        }
        catch (Exception ex)
        {
            result.IsConnected = false;
            result.ResponseTime = DateTime.Now - result.StartTime;
            result.ErrorMessage = ex.Message;
            result.Message = "数据库连接失败";
        }

        return result;
    }

    /// <summary>
    /// 获取简短的连接状态消息
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetConnectionStatusAsync()
    {
        var result = await CheckConnectionAsync();
        
        if (result.IsConnected)
        {
            var status = result.DatabaseExists ? 
                (result.TablesExist ? "数据库就绪" : "数据库存在，表未创建") : 
                "数据库不存在";
            return $"✓ 数据库连接成功 - {status} ({result.ResponseTime.TotalMilliseconds:F0}ms)";
        }
        else
        {
            return $"✗ 数据库连接失败 - {result.ErrorMessage}";
        }
    }
}

/// <summary>
/// 数据库连接结果
/// </summary>
public class DatabaseConnectionResult
{
    /// <summary>
    /// 是否连接成功
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 连接消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 服务器版本
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public uint ConnectionId { get; set; }

    /// <summary>
    /// 服务器时间
    /// </summary>
    public DateTime? ServerTime { get; set; }

    /// <summary>
    /// 数据库是否存在
    /// </summary>
    public bool DatabaseExists { get; set; }

    /// <summary>
    /// 表是否存在
    /// </summary>
    public bool TablesExist { get; set; }

    /// <summary>
    /// 响应时间
    /// </summary>
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 机器码（完整32位）
    /// </summary>
    public string MachineCode { get; set; } = string.Empty;

    /// <summary>
    /// 获取详细状态信息
    /// </summary>
    /// <returns></returns>
    public string GetDetailedStatus()
    {
        if (!IsConnected)
        {
            return $"连接失败: {ErrorMessage}";
        }

        var details = new List<string>
        {
            $"服务器: {DatabaseConfig.Server}:{DatabaseConfig.Port}",
            $"版本: {ServerVersion}",
            $"响应时间: {ResponseTime.TotalMilliseconds:F0}ms",
            $"机器码: {MachineCode}"
        };

        if (DatabaseExists)
        {
            details.Add($"数据库: {DatabaseConfig.Database} ✓");
            details.Add($"数据表: {(TablesExist ? "已创建 ✓" : "未创建 ⚠")}");
        }
        else
        {
            details.Add($"数据库: {DatabaseConfig.Database} ✗");
        }

        return string.Join(" | ", details);
    }
}
