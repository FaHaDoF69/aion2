using System;

namespace Aion2Helper.Config;

/// <summary>
/// 数据库配置类
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public static string ConnectionString { get; } = 
        "Server=112.111.39.216;Port=3309;Database=aion2;User=aion2;Password=PwpxLk3cKMwGDzc4;CharSet=utf8mb4;SslMode=Required;";

    /// <summary>
    /// 数据库服务器地址
    /// </summary>
    public static string Server { get; } = "112.111.39.216";

    /// <summary>
    /// 数据库端口
    /// </summary>
    public static int Port { get; } = 3309;

    /// <summary>
    /// 数据库名称
    /// </summary>
    public static string Database { get; } = "aion2";

    /// <summary>
    /// 用户名
    /// </summary>
    public static string Username { get; } = "aion2";

    /// <summary>
    /// 密码
    /// </summary>
    public static string Password { get; } = "PwpxLk3cKMwGDzc4";

    /// <summary>
    /// 字符集
    /// </summary>
    public static string CharSet { get; } = "utf8mb4";

    /// <summary>
    /// SSL模式
    /// </summary>
    public static string SslMode { get; } = "Required";

    /// <summary>
    /// 连接超时时间（秒）
    /// </summary>
    public static int ConnectionTimeout { get; } = 30;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    public static int CommandTimeout { get; } = 60;
}
