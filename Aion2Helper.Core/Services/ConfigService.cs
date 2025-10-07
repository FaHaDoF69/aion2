using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aion2Helper.Models;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 配置服务 - 管理用户设置的保存和加载
    /// </summary>
    public class ConfigService
    {
        private readonly string _configFilePath;
        private readonly string _configDirectory;
        private UserConfig _config = new();
        private readonly object _lockObject = new();

        public event Action<string>? LogMessage;

        public ConfigService()
        {
            // 配置文件保存在应用程序目录下的 config 文件夹
            var appDir = Path.GetDirectoryName(Application.ExecutablePath) ?? Environment.CurrentDirectory;
            _configDirectory = Path.Combine(appDir, "config");
            _configFilePath = Path.Combine(_configDirectory, "user_config.json");
            
            // 确保配置目录存在
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
            
            // 加载配置
            LoadConfig();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            lock (_lockObject)
            {
                try
                {
                    if (File.Exists(_configFilePath))
                    {
                        var json = File.ReadAllText(_configFilePath, System.Text.Encoding.UTF8);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true
                        };
                        _config = JsonSerializer.Deserialize<UserConfig>(json, options) ?? new UserConfig();
                        
                        LogMessage?.Invoke($"✅ 配置加载成功: {_configFilePath}");
                        
                        // 验证配置完整性
                        ValidateConfig();
                    }
                    else
                    {
                        _config = new UserConfig();
                        LogMessage?.Invoke("📝 使用默认配置，配置文件将在首次保存时创建");
                        
                        // 创建默认配置文件
                        SaveConfig();
                    }
                }
                catch (Exception ex)
                {
                    // 配置文件损坏时创建新的
                    _config = new UserConfig();
                    LogMessage?.Invoke($"❌ 加载配置失败，使用默认配置: {ex.Message}");
                    
                    // 备份损坏的配置文件
                    BackupCorruptedConfig();
                }
            }
        }

        /// <summary>
        /// 验证配置完整性
        /// </summary>
        private void ValidateConfig()
        {
            bool needsSave = false;

            // 确保所有子配置对象不为空
            if (_config.Database == null)
            {
                _config.Database = new DatabaseSettings();
                needsSave = true;
            }

            if (_config.Monitoring == null)
            {
                _config.Monitoring = new MonitoringSettings();
                needsSave = true;
            }

            if (_config.Notification == null)
            {
                _config.Notification = new NotificationSettings();
                needsSave = true;
            }

            if (_config.ImageRecognition == null)
            {
                _config.ImageRecognition = new ImageRecognitionSettings();
                needsSave = true;
            }

            if (_config.Advanced == null)
            {
                _config.Advanced = new AdvancedSettings();
                needsSave = true;
            }

            if (_config.UI == null)
            {
                _config.UI = new UISettings();
                needsSave = true;
            }

            // 验证数值范围
            if (_config.MonitorInterval < 1000)
            {
                _config.MonitorInterval = 1000;
                needsSave = true;
            }

            if (_config.MaxDisplayItems < 10)
            {
                _config.MaxDisplayItems = 10;
                needsSave = true;
            }

            if (_config.ImageRecognitionThreshold < 0.1 || _config.ImageRecognitionThreshold > 1.0)
            {
                _config.ImageRecognitionThreshold = 0.8;
                needsSave = true;
            }

            if (needsSave)
            {
                SaveConfig();
                LogMessage?.Invoke("🔧 配置已自动修复并保存");
            }
        }

        /// <summary>
        /// 备份损坏的配置文件
        /// </summary>
        private void BackupCorruptedConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var backupPath = Path.Combine(_configDirectory, $"user_config_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                    File.Copy(_configFilePath, backupPath);
                    LogMessage?.Invoke($"💾 已备份损坏的配置文件到: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"⚠️ 备份配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置（异步版本）
        /// </summary>
        public async Task SaveConfigAsync()
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(_config, options);
                await File.WriteAllTextAsync(_configFilePath, json, System.Text.Encoding.UTF8);
                
                LogMessage?.Invoke($"💾 配置保存成功: {_configFilePath}");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 保存配置失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 保存配置（同步版本）
        /// </summary>
        public void SaveConfig()
        {
            lock (_lockObject)
            {
                try
                {
                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    var json = JsonSerializer.Serialize(_config, options);
                    File.WriteAllText(_configFilePath, json, System.Text.Encoding.UTF8);
                    
                    LogMessage?.Invoke($"💾 配置保存成功");
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"❌ 保存配置失败: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 获取完整配置对象
        /// </summary>
        public UserConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 更新完整配置对象
        /// </summary>
        public void UpdateConfig(UserConfig config)
        {
            lock (_lockObject)
            {
                _config = config ?? new UserConfig();
                SaveConfig();
            }
        }

        #region 基础设置

        /// <summary>
        /// 获取监控间隔（毫秒）
        /// </summary>
        public int GetMonitorInterval()
        {
            return _config.MonitorInterval;
        }

        /// <summary>
        /// 设置监控间隔
        /// </summary>
        public void SetMonitorInterval(int intervalMs)
        {
            _config.MonitorInterval = Math.Max(1000, intervalMs); // 最小1秒
            SaveConfig();
        }

        /// <summary>
        /// 获取最大显示物品数量
        /// </summary>
        public int GetMaxDisplayItems()
        {
            return _config.MaxDisplayItems;
        }

        /// <summary>
        /// 设置最大显示物品数量
        /// </summary>
        public void SetMaxDisplayItems(int maxItems)
        {
            _config.MaxDisplayItems = Math.Max(10, maxItems);
            SaveConfig();
        }

        /// <summary>
        /// 获取调试模式状态
        /// </summary>
        public bool GetDebugMode()
        {
            return _config.DebugMode;
        }

        /// <summary>
        /// 设置调试模式状态
        /// </summary>
        public void SetDebugMode(bool debugMode)
        {
            _config.DebugMode = debugMode;
            SaveConfig();
        }

        /// <summary>
        /// 获取窗体位置和大小
        /// </summary>
        public (int X, int Y, int Width, int Height) GetWindowBounds()
        {
            return (_config.WindowX, _config.WindowY, _config.WindowWidth, _config.WindowHeight);
        }

        /// <summary>
        /// 设置窗体位置和大小
        /// </summary>
        public void SetWindowBounds(int x, int y, int width, int height)
        {
            _config.WindowX = x;
            _config.WindowY = y;
            _config.WindowWidth = Math.Max(800, width);
            _config.WindowHeight = Math.Max(600, height);
            SaveConfig();
        }

        /// <summary>
        /// 获取游戏窗口标题
        /// </summary>
        public string GetGameWindowTitle()
        {
            return _config.GameWindowTitle;
        }

        /// <summary>
        /// 设置游戏窗口标题
        /// </summary>
        public void SetGameWindowTitle(string title)
        {
            _config.GameWindowTitle = title ?? "Aion2";
            SaveConfig();
        }

        #endregion

        #region 数据库设置

        /// <summary>
        /// 获取数据库设置
        /// </summary>
        public DatabaseSettings GetDatabaseSettings()
        {
            return _config.Database;
        }

        /// <summary>
        /// 设置数据库设置
        /// </summary>
        public void SetDatabaseSettings(DatabaseSettings settings)
        {
            _config.Database = settings ?? new DatabaseSettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取是否使用自定义数据库连接
        /// </summary>
        public bool GetUseCustomDatabaseConnection()
        {
            return _config.Database.UseCustomConnection;
        }

        /// <summary>
        /// 设置是否使用自定义数据库连接
        /// </summary>
        public void SetUseCustomDatabaseConnection(bool useCustom)
        {
            _config.Database.UseCustomConnection = useCustom;
            SaveConfig();
        }

        #endregion

        #region 监控设置

        /// <summary>
        /// 获取监控设置
        /// </summary>
        public MonitoringSettings GetMonitoringSettings()
        {
            return _config.Monitoring;
        }

        /// <summary>
        /// 设置监控设置
        /// </summary>
        public void SetMonitoringSettings(MonitoringSettings settings)
        {
            _config.Monitoring = settings ?? new MonitoringSettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取自动启动监控
        /// </summary>
        public bool GetAutoStartMonitoring()
        {
            return _config.AutoStartMonitoring;
        }

        /// <summary>
        /// 设置自动启动监控
        /// </summary>
        public void SetAutoStartMonitoring(bool autoStart)
        {
            _config.AutoStartMonitoring = autoStart;
            SaveConfig();
        }

        /// <summary>
        /// 获取自动购买设置
        /// </summary>
        public bool GetAutoPurchase()
        {
            return _config.Monitoring.AutoPurchase;
        }

        /// <summary>
        /// 设置自动购买
        /// </summary>
        public void SetAutoPurchase(bool autoPurchase)
        {
            _config.Monitoring.AutoPurchase = autoPurchase;
            SaveConfig();
        }

        #endregion

        #region 通知设置

        /// <summary>
        /// 获取通知设置
        /// </summary>
        public NotificationSettings GetNotificationSettings()
        {
            return _config.Notification;
        }

        /// <summary>
        /// 设置通知设置
        /// </summary>
        public void SetNotificationSettings(NotificationSettings settings)
        {
            _config.Notification = settings ?? new NotificationSettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取声音通知设置
        /// </summary>
        public bool GetSoundNotification()
        {
            return _config.SoundNotification;
        }

        /// <summary>
        /// 设置声音通知
        /// </summary>
        public void SetSoundNotification(bool enabled)
        {
            _config.SoundNotification = enabled;
            SaveConfig();
        }

        /// <summary>
        /// 获取弹窗通知设置
        /// </summary>
        public bool GetPopupNotification()
        {
            return _config.PopupNotification;
        }

        /// <summary>
        /// 设置弹窗通知
        /// </summary>
        public void SetPopupNotification(bool enabled)
        {
            _config.PopupNotification = enabled;
            SaveConfig();
        }

        #endregion

        #region 图像识别设置

        /// <summary>
        /// 获取图像识别设置
        /// </summary>
        public ImageRecognitionSettings GetImageRecognitionSettings()
        {
            return _config.ImageRecognition;
        }

        /// <summary>
        /// 设置图像识别设置
        /// </summary>
        public void SetImageRecognitionSettings(ImageRecognitionSettings settings)
        {
            _config.ImageRecognition = settings ?? new ImageRecognitionSettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取图像识别阈值
        /// </summary>
        public double GetImageRecognitionThreshold()
        {
            return _config.ImageRecognitionThreshold;
        }

        /// <summary>
        /// 设置图像识别阈值
        /// </summary>
        public void SetImageRecognitionThreshold(double threshold)
        {
            _config.ImageRecognitionThreshold = Math.Max(0.1, Math.Min(1.0, threshold));
            SaveConfig();
        }

        /// <summary>
        /// 获取是否保存调试截图
        /// </summary>
        public bool GetSaveDebugScreenshots()
        {
            return _config.SaveDebugScreenshots;
        }

        /// <summary>
        /// 设置是否保存调试截图
        /// </summary>
        public void SetSaveDebugScreenshots(bool save)
        {
            _config.SaveDebugScreenshots = save;
            SaveConfig();
        }

        #endregion

        #region 高级设置

        /// <summary>
        /// 获取高级设置
        /// </summary>
        public AdvancedSettings GetAdvancedSettings()
        {
            return _config.Advanced;
        }

        /// <summary>
        /// 设置高级设置
        /// </summary>
        public void SetAdvancedSettings(AdvancedSettings settings)
        {
            _config.Advanced = settings ?? new AdvancedSettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取日志级别
        /// </summary>
        public int GetLogLevel()
        {
            return _config.Advanced.LogLevel;
        }

        /// <summary>
        /// 设置日志级别
        /// </summary>
        public void SetLogLevel(int level)
        {
            _config.Advanced.LogLevel = Math.Max(0, Math.Min(4, level));
            SaveConfig();
        }

        #endregion

        #region UI设置

        /// <summary>
        /// 获取UI设置
        /// </summary>
        public UISettings GetUISettings()
        {
            return _config.UI;
        }

        /// <summary>
        /// 设置UI设置
        /// </summary>
        public void SetUISettings(UISettings settings)
        {
            _config.UI = settings ?? new UISettings();
            SaveConfig();
        }

        /// <summary>
        /// 获取主题
        /// </summary>
        public string GetTheme()
        {
            return _config.UI.Theme;
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        public void SetTheme(string theme)
        {
            _config.UI.Theme = theme ?? "Auto";
            SaveConfig();
        }

        /// <summary>
        /// 获取语言
        /// </summary>
        public string GetLanguage()
        {
            return _config.UI.Language;
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public void SetLanguage(string language)
        {
            _config.UI.Language = language ?? "zh-CN";
            SaveConfig();
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 重置配置到默认值
        /// </summary>
        public void ResetToDefault()
        {
            lock (_lockObject)
            {
                _config = new UserConfig();
                SaveConfig();
                LogMessage?.Invoke("🔄 配置已重置为默认值");
            }
        }

        /// <summary>
        /// 导出配置到文件
        /// </summary>
        public void ExportConfig(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
                
                LogMessage?.Invoke($"📤 配置导出成功: {filePath}");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 导出配置失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 从文件导入配置
        /// </summary>
        public void ImportConfig(string filePath)
        {
            lock (_lockObject)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"配置文件不存在: {filePath}");
                    }

                    var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    
                    var importedConfig = JsonSerializer.Deserialize<UserConfig>(json, options);
                    if (importedConfig != null)
                    {
                        _config = importedConfig;
                        ValidateConfig();
                        SaveConfig();
                        LogMessage?.Invoke($"📥 配置导入成功: {filePath}");
                    }
                    else
                    {
                        throw new InvalidOperationException("配置文件格式无效");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"❌ 导入配置失败: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public string GetConfigFilePath()
        {
            return _configFilePath;
        }

        /// <summary>
        /// 获取配置目录路径
        /// </summary>
        public string GetConfigDirectory()
        {
            return _configDirectory;
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        public bool ConfigFileExists()
        {
            return File.Exists(_configFilePath);
        }

        /// <summary>
        /// 获取配置文件大小
        /// </summary>
        public long GetConfigFileSize()
        {
            return File.Exists(_configFilePath) ? new FileInfo(_configFilePath).Length : 0;
        }

        /// <summary>
        /// 获取配置最后修改时间
        /// </summary>
        public DateTime GetConfigLastModified()
        {
            return File.Exists(_configFilePath) ? File.GetLastWriteTime(_configFilePath) : DateTime.MinValue;
        }

        #endregion
    }
}
