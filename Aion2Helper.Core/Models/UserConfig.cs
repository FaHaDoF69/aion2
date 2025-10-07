using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aion2Helper.Models
{
    /// <summary>
    /// 用户配置模型
    /// </summary>
    public class UserConfig
    {
        /// <summary>
        /// 监控间隔（毫秒）
        /// </summary>
        public int MonitorInterval { get; set; } = 5000; // 默认5秒

        /// <summary>
        /// 最大显示物品数量
        /// </summary>
        public int MaxDisplayItems { get; set; } = 100;

        /// <summary>
        /// 窗体X坐标
        /// </summary>
        public int WindowX { get; set; } = -1;

        /// <summary>
        /// 窗体Y坐标
        /// </summary>
        public int WindowY { get; set; } = -1;

        /// <summary>
        /// 窗体宽度
        /// </summary>
        public int WindowWidth { get; set; } = 1200;

        /// <summary>
        /// 窗体高度
        /// </summary>
        public int WindowHeight { get; set; } = 700;

        /// <summary>
        /// 调试模式开关
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// 自动启动监控
        /// </summary>
        public bool AutoStartMonitoring { get; set; } = false;

        /// <summary>
        /// 最小化到系统托盘
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// 启动时最小化
        /// </summary>
        public bool StartMinimized { get; set; } = false;

        /// <summary>
        /// 声音通知
        /// </summary>
        public bool SoundNotification { get; set; } = true;

        /// <summary>
        /// 弹窗通知
        /// </summary>
        public bool PopupNotification { get; set; } = true;

        /// <summary>
        /// 游戏窗口标题
        /// </summary>
        public string GameWindowTitle { get; set; } = "Aion2";

        /// <summary>
        /// 自动检测游戏窗口
        /// </summary>
        public bool AutoDetectGameWindow { get; set; } = true;

        /// <summary>
        /// 图像识别阈值
        /// </summary>
        public double ImageRecognitionThreshold { get; set; } = 0.8;

        /// <summary>
        /// 截图保存路径
        /// </summary>
        public string ScreenshotPath { get; set; } = "screenshots";

        /// <summary>
        /// 是否保存调试截图
        /// </summary>
        public bool SaveDebugScreenshots { get; set; } = false;

        /// <summary>
        /// 数据库连接配置
        /// </summary>
        public DatabaseSettings Database { get; set; } = new();

        /// <summary>
        /// 监控设置
        /// </summary>
        public MonitoringSettings Monitoring { get; set; } = new();

        /// <summary>
        /// 通知设置
        /// </summary>
        public NotificationSettings Notification { get; set; } = new();

        /// <summary>
        /// 图像识别设置
        /// </summary>
        public ImageRecognitionSettings ImageRecognition { get; set; } = new();

        /// <summary>
        /// 高级设置
        /// </summary>
        public AdvancedSettings Advanced { get; set; } = new();

        /// <summary>
        /// 用户界面设置
        /// </summary>
        public UISettings UI { get; set; } = new();
    }

    /// <summary>
    /// 数据库设置
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// 是否使用自定义数据库连接
        /// </summary>
        public bool UseCustomConnection { get; set; } = false;

        /// <summary>
        /// 自定义连接字符串
        /// </summary>
        public string? CustomConnectionString { get; set; }

        /// <summary>
        /// 数据库服务器地址
        /// </summary>
        public string Server { get; set; } = "localhost";

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int Port { get; set; } = 3306;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName { get; set; } = "aion2";

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = "root";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// 命令超时时间（秒）
        /// </summary>
        public int CommandTimeout { get; set; } = 60;
    }

    /// <summary>
    /// 监控设置
    /// </summary>
    public class MonitoringSettings
    {
        /// <summary>
        /// 监控的物品列表
        /// </summary>
        public List<string> MonitoredItemNames { get; set; } = new();

        /// <summary>
        /// 价格阈值监控
        /// </summary>
        public bool EnablePriceThreshold { get; set; } = true;

        /// <summary>
        /// 数量阈值监控
        /// </summary>
        public bool EnableQuantityThreshold { get; set; } = true;

        /// <summary>
        /// 自动购买
        /// </summary>
        public bool AutoPurchase { get; set; } = false;

        /// <summary>
        /// 购买延迟（毫秒）
        /// </summary>
        public int PurchaseDelay { get; set; } = 1000;

        /// <summary>
        /// 最大购买数量
        /// </summary>
        public int MaxPurchaseQuantity { get; set; } = 1;

        /// <summary>
        /// 购买策略（0=最便宜，1=最快速，2=随机）
        /// </summary>
        public int PurchaseStrategy { get; set; } = 0;
    }

    /// <summary>
    /// 通知设置
    /// </summary>
    public class NotificationSettings
    {
        /// <summary>
        /// 邮件通知是否启用
        /// </summary>
        public bool EmailEnabled { get; set; } = false;

        /// <summary>
        /// 发件人邮箱
        /// </summary>
        public string EmailSender { get; set; } = "";

        /// <summary>
        /// 发件人邮箱密码/授权码
        /// </summary>
        public string EmailPassword { get; set; } = "";

        /// <summary>
        /// 收件人邮箱
        /// </summary>
        public string EmailReceiver { get; set; } = "";

        /// <summary>
        /// SMTP服务器
        /// </summary>
        public string EmailSmtpServer { get; set; } = "smtp.qq.com";

        /// <summary>
        /// SMTP端口
        /// </summary>
        public int EmailSmtpPort { get; set; } = 587;

        /// <summary>
        /// 微信通知（企业微信机器人）
        /// </summary>
        public bool WeChatEnabled { get; set; } = false;

        /// <summary>
        /// 企业微信机器人Webhook URL
        /// </summary>
        public string WeChatWebhookUrl { get; set; } = "";

        /// <summary>
        /// 钉钉通知
        /// </summary>
        public bool DingTalkEnabled { get; set; } = false;

        /// <summary>
        /// 钉钉机器人Webhook URL
        /// </summary>
        public string DingTalkWebhookUrl { get; set; } = "";

        /// <summary>
        /// 通知频率限制（分钟）
        /// </summary>
        public int NotificationCooldown { get; set; } = 5;
    }

    /// <summary>
    /// 图像识别设置
    /// </summary>
    public class ImageRecognitionSettings
    {
        /// <summary>
        /// 模板匹配阈值
        /// </summary>
        public double TemplateMatchThreshold { get; set; } = 0.8;

        /// <summary>
        /// 颜色识别容差
        /// </summary>
        public int ColorTolerance { get; set; } = 10;

        /// <summary>
        /// 截图质量（1-100）
        /// </summary>
        public int ScreenshotQuality { get; set; } = 90;

        /// <summary>
        /// 图像预处理
        /// </summary>
        public bool EnableImagePreprocessing { get; set; } = true;

        /// <summary>
        /// 边缘检测
        /// </summary>
        public bool EnableEdgeDetection { get; set; } = false;

        /// <summary>
        /// 模糊检测阈值
        /// </summary>
        public double BlurThreshold { get; set; } = 100.0;

        /// <summary>
        /// 自动调整识别参数
        /// </summary>
        public bool AutoAdjustParameters { get; set; } = true;
    }

    /// <summary>
    /// 高级设置
    /// </summary>
    public class AdvancedSettings
    {
        /// <summary>
        /// 多线程处理
        /// </summary>
        public bool EnableMultiThreading { get; set; } = true;

        /// <summary>
        /// 最大线程数
        /// </summary>
        public int MaxThreadCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// 内存缓存大小（MB）
        /// </summary>
        public int MemoryCacheSize { get; set; } = 100;

        /// <summary>
        /// 日志级别（0=关闭，1=错误，2=警告，3=信息，4=调试）
        /// </summary>
        public int LogLevel { get; set; } = 2;

        /// <summary>
        /// 日志文件保留天数
        /// </summary>
        public int LogRetentionDays { get; set; } = 7;

        /// <summary>
        /// 性能监控
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = false;

        /// <summary>
        /// 自动更新检查
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// 崩溃报告
        /// </summary>
        public bool EnableCrashReporting { get; set; } = true;
    }

    /// <summary>
    /// 用户界面设置
    /// </summary>
    public class UISettings
    {
        /// <summary>
        /// 主题（Light, Dark, Auto）
        /// </summary>
        public string Theme { get; set; } = "Auto";

        /// <summary>
        /// 语言（zh-CN, en-US）
        /// </summary>
        public string Language { get; set; } = "zh-CN";

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { get; set; } = 9;

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontFamily { get; set; } = "Microsoft YaHei UI";

        /// <summary>
        /// 显示网格线
        /// </summary>
        public bool ShowGridLines { get; set; } = true;

        /// <summary>
        /// 显示状态栏
        /// </summary>
        public bool ShowStatusBar { get; set; } = true;

        /// <summary>
        /// 显示工具栏
        /// </summary>
        public bool ShowToolBar { get; set; } = true;

        /// <summary>
        /// 自动保存窗口位置
        /// </summary>
        public bool AutoSaveWindowPosition { get; set; } = true;

        /// <summary>
        /// 列表刷新动画
        /// </summary>
        public bool EnableListAnimation { get; set; } = true;

        /// <summary>
        /// 透明度（0-100）
        /// </summary>
        public int WindowOpacity { get; set; } = 100;
    }

    /// <summary>
    /// 监控历史项
    /// </summary>
    public class MonitoringHistoryItem
    {
        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 监控开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 监控结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        public int TriggerCount { get; set; }

        /// <summary>
        /// 最后触发时间
        /// </summary>
        public DateTime? LastTriggerTime { get; set; }
    }

    /// <summary>
    /// 快捷设置预设
    /// </summary>
    public class QuickSettingsPreset
    {
        /// <summary>
        /// 预设名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 预设描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 监控间隔
        /// </summary>
        public int MonitorInterval { get; set; }

        /// <summary>
        /// 图像识别阈值
        /// </summary>
        public double ImageThreshold { get; set; }

        /// <summary>
        /// 是否启用自动购买
        /// </summary>
        public bool AutoPurchase { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}
