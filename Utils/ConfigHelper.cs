using System;
using System.IO;
using System.Text.Json;
using Aion2Helper.Models;
using Aion2Helper.Services;

namespace Aion2Helper.Utils
{
    /// <summary>
    /// 配置辅助工具类
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static UserConfig CreateDefaultConfig()
        {
            return new UserConfig
            {
                MonitorInterval = 5000,
                MaxDisplayItems = 100,
                DebugMode = false,
                AutoStartMonitoring = false,
                MinimizeToTray = true,
                StartMinimized = false,
                SoundNotification = true,
                PopupNotification = true,
                GameWindowTitle = "Aion2",
                AutoDetectGameWindow = true,
                ImageRecognitionThreshold = 0.8,
                ScreenshotPath = "screenshots",
                SaveDebugScreenshots = false,
                
                Database = new DatabaseSettings
                {
                    UseCustomConnection = false,
                    Server = "localhost",
                    Port = 3306,
                    DatabaseName = "aion2",
                    Username = "root",
                    Password = "",
                    ConnectionTimeout = 30,
                    CommandTimeout = 60
                },
                
                Monitoring = new MonitoringSettings
                {
                    MonitoredItemNames = new(),
                    EnablePriceThreshold = true,
                    EnableQuantityThreshold = true,
                    AutoPurchase = false,
                    PurchaseDelay = 1000,
                    MaxPurchaseQuantity = 1,
                    PurchaseStrategy = 0
                },
                
                Notification = new NotificationSettings
                {
                    EmailEnabled = false,
                    EmailSender = "",
                    EmailPassword = "",
                    EmailReceiver = "",
                    EmailSmtpServer = "smtp.qq.com",
                    EmailSmtpPort = 587,
                    WeChatEnabled = false,
                    WeChatWebhookUrl = "",
                    DingTalkEnabled = false,
                    DingTalkWebhookUrl = "",
                    NotificationCooldown = 5
                },
                
                ImageRecognition = new ImageRecognitionSettings
                {
                    TemplateMatchThreshold = 0.8,
                    ColorTolerance = 10,
                    ScreenshotQuality = 90,
                    EnableImagePreprocessing = true,
                    EnableEdgeDetection = false,
                    BlurThreshold = 100.0,
                    AutoAdjustParameters = true
                },
                
                Advanced = new AdvancedSettings
                {
                    EnableMultiThreading = true,
                    MaxThreadCount = Environment.ProcessorCount,
                    MemoryCacheSize = 100,
                    LogLevel = 2,
                    LogRetentionDays = 7,
                    EnablePerformanceMonitoring = false,
                    CheckForUpdates = true,
                    EnableCrashReporting = true
                },
                
                UI = new UISettings
                {
                    Theme = "Auto",
                    Language = "zh-CN",
                    FontSize = 9,
                    FontFamily = "Microsoft YaHei UI",
                    ShowGridLines = true,
                    ShowStatusBar = true,
                    ShowToolBar = true,
                    AutoSaveWindowPosition = true,
                    EnableListAnimation = true,
                    WindowOpacity = 100
                }
            };
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public static (bool IsValid, string[] Errors) ValidateConfig(UserConfig config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("配置对象为空");
                return (false, errors.ToArray());
            }

            // 验证基础设置
            if (config.MonitorInterval < 1000)
                errors.Add("监控间隔不能小于1秒");

            if (config.MaxDisplayItems < 1)
                errors.Add("最大显示物品数量不能小于1");

            if (config.ImageRecognitionThreshold < 0.1 || config.ImageRecognitionThreshold > 1.0)
                errors.Add("图像识别阈值必须在0.1-1.0之间");

            // 验证窗口设置
            if (config.WindowWidth < 800)
                errors.Add("窗口宽度不能小于800像素");

            if (config.WindowHeight < 600)
                errors.Add("窗口高度不能小于600像素");

            // 验证数据库设置
            if (config.Database?.UseCustomConnection == true)
            {
                if (string.IsNullOrWhiteSpace(config.Database.Server))
                    errors.Add("数据库服务器地址不能为空");

                if (config.Database.Port < 1 || config.Database.Port > 65535)
                    errors.Add("数据库端口必须在1-65535之间");

                if (string.IsNullOrWhiteSpace(config.Database.DatabaseName))
                    errors.Add("数据库名称不能为空");
            }

            // 验证通知设置
            if (config.Notification?.EmailEnabled == true)
            {
                if (string.IsNullOrWhiteSpace(config.Notification.EmailSender))
                    errors.Add("发件人邮箱不能为空");

                if (string.IsNullOrWhiteSpace(config.Notification.EmailReceiver))
                    errors.Add("收件人邮箱不能为空");

                if (string.IsNullOrWhiteSpace(config.Notification.EmailSmtpServer))
                    errors.Add("SMTP服务器不能为空");

                if (config.Notification.EmailSmtpPort < 1 || config.Notification.EmailSmtpPort > 65535)
                    errors.Add("SMTP端口必须在1-65535之间");
            }

            // 验证高级设置
            if (config.Advanced != null)
            {
                if (config.Advanced.MaxThreadCount < 1)
                    errors.Add("最大线程数不能小于1");

                if (config.Advanced.MemoryCacheSize < 10)
                    errors.Add("内存缓存大小不能小于10MB");

                if (config.Advanced.LogLevel < 0 || config.Advanced.LogLevel > 4)
                    errors.Add("日志级别必须在0-4之间");
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <summary>
        /// 合并配置（用于配置升级）
        /// </summary>
        public static UserConfig MergeConfigs(UserConfig baseConfig, UserConfig newConfig)
        {
            if (baseConfig == null) return newConfig ?? CreateDefaultConfig();
            if (newConfig == null) return baseConfig;

            // 使用 JSON 序列化/反序列化进行深度合并
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                // 序列化基础配置
                var baseJson = JsonSerializer.Serialize(baseConfig, options);
                var baseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(baseJson, options);

                // 序列化新配置
                var newJson = JsonSerializer.Serialize(newConfig, options);
                var newDict = JsonSerializer.Deserialize<Dictionary<string, object>>(newJson, options);

                // 合并字典
                if (baseDict != null && newDict != null)
                {
                    foreach (var kvp in newDict)
                    {
                        baseDict[kvp.Key] = kvp.Value;
                    }

                    // 反序列化回配置对象
                    var mergedJson = JsonSerializer.Serialize(baseDict, options);
                    return JsonSerializer.Deserialize<UserConfig>(mergedJson, options) ?? baseConfig;
                }
            }
            catch
            {
                // 合并失败时返回新配置
            }

            return newConfig;
        }

        /// <summary>
        /// 获取配置摘要信息
        /// </summary>
        public static string GetConfigSummary(UserConfig config)
        {
            if (config == null) return "配置为空";

            var summary = new List<string>
            {
                $"监控间隔: {config.MonitorInterval}ms",
                $"最大显示: {config.MaxDisplayItems}项",
                $"调试模式: {(config.DebugMode ? "开启" : "关闭")}",
                $"自动监控: {(config.AutoStartMonitoring ? "开启" : "关闭")}",
                $"声音通知: {(config.SoundNotification ? "开启" : "关闭")}",
                $"弹窗通知: {(config.PopupNotification ? "开启" : "关闭")}",
                $"图像阈值: {config.ImageRecognitionThreshold:P0}",
                $"主题: {config.UI?.Theme ?? "未设置"}",
                $"语言: {config.UI?.Language ?? "未设置"}"
            };

            return string.Join(" | ", summary);
        }

        /// <summary>
        /// 检查配置版本兼容性
        /// </summary>
        public static bool IsConfigVersionCompatible(string configJson)
        {
            try
            {
                using var document = JsonDocument.Parse(configJson);
                var root = document.RootElement;

                // 检查是否包含必要的字段
                var requiredFields = new[] { "MonitorInterval", "MaxDisplayItems", "DebugMode" };
                
                foreach (var field in requiredFields)
                {
                    if (!root.TryGetProperty(field, out _))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 生成配置备份文件名
        /// </summary>
        public static string GenerateBackupFileName(string baseName = "user_config")
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{baseName}_backup_{timestamp}.json";
        }

        /// <summary>
        /// 清理过期的配置备份文件
        /// </summary>
        public static void CleanupOldBackups(string configDirectory, int keepDays = 30)
        {
            try
            {
                if (!Directory.Exists(configDirectory)) return;

                var backupFiles = Directory.GetFiles(configDirectory, "*_backup_*.json");
                var cutoffDate = DateTime.Now.AddDays(-keepDays);

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // 忽略删除失败的文件
                        }
                    }
                }
            }
            catch
            {
                // 忽略清理过程中的异常
            }
        }

        /// <summary>
        /// 获取推荐的配置设置（基于系统性能）
        /// </summary>
        public static UserConfig GetRecommendedConfig()
        {
            var config = CreateDefaultConfig();

            // 根据系统性能调整设置
            var processorCount = Environment.ProcessorCount;
            var totalMemory = GC.GetTotalMemory(false);

            // 调整线程数
            config.Advanced.MaxThreadCount = Math.Max(2, Math.Min(processorCount, 8));

            // 调整缓存大小
            if (totalMemory > 1024 * 1024 * 1024) // > 1GB
            {
                config.Advanced.MemoryCacheSize = 200;
            }
            else
            {
                config.Advanced.MemoryCacheSize = 50;
            }

            // 调整监控间隔
            if (processorCount >= 8)
            {
                config.MonitorInterval = 3000; // 高性能系统可以更频繁
            }
            else if (processorCount >= 4)
            {
                config.MonitorInterval = 5000; // 中等性能
            }
            else
            {
                config.MonitorInterval = 8000; // 低性能系统降低频率
            }

            return config;
        }
    }
}
