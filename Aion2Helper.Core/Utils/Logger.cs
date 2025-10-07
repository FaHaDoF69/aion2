using System;

namespace Aion2Helper.Utils;

/// <summary>
/// 通用日志记录器
/// </summary>
public static class Logger
{
    /// <summary>
    /// 记录错误日志
    /// </summary>
    /// <param name="module">模块名称</param>
    /// <param name="message">消息内容</param>
    public static void LogError(string module, string message)
    {
        Console.WriteLine($"[错误] [{module}] {message}");
        
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"[错误] [{module}] {message}");
        #endif
    }

    /// <summary>
    /// 记录信息日志
    /// </summary>
    /// <param name="module">模块名称</param>
    /// <param name="message">消息内容</param>
    public static void LogInfo(string module, string message)
    {
        Console.WriteLine($"[信息] [{module}] {message}");
        
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"[信息] [{module}] {message}");
        #endif
    }

    /// <summary>
    /// 记录警告日志
    /// </summary>
    /// <param name="module">模块名称</param>
    /// <param name="message">消息内容</param>
    public static void LogWarning(string module, string message)
    {
        Console.WriteLine($"[警告] [{module}] {message}");
        
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"[警告] [{module}] {message}");
        #endif
    }
}

