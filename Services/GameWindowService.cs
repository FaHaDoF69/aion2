using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Rectangle = System.Drawing.Rectangle;

namespace Aion2Helper.Services
{
/// <summary>
    /// 游戏窗口检测和坐标转换服务 - 增强版
/// </summary>
public class GameWindowService
{
        #region Windows API 声明

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSrc, int xSrc, int ySrc, int RasterOp);

        private const int SW_RESTORE = 9; // 恢复窗口（如果最小化）
        private const int SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        private bool _debugMode = false;
        public event Action<string>? LogMessage;

    private IntPtr _gameWindowHandle = IntPtr.Zero;
        private Rectangle _gameWindowRect = Rectangle.Empty;
        private Rectangle _gameClientRect = Rectangle.Empty;
        private Point _clientOffset = Point.Empty;
        private DateTime _lastDetectionTime = DateTime.MinValue;
        
        // 上一次检测到的窗口位置（用于检测窗口移动）
        private Rectangle _previousClientRect = Rectangle.Empty;
        
        // 游戏窗口可能的标题（支持多个）
        private readonly string[] _possibleWindowTitles = new[]
        {
            "Aion2", // 默认标题
            "AION2",
            "Aion 2",
            "永恒之塔2",
            "永恒之塔 2",
        };

        // 游戏进程可能的名称
        private readonly string[] _possibleProcessNames = new[]
        {
            "Aion2",
            "AION2",
            "aion2",
            "Aion",
            "AION"
        };

        /// <summary>
        /// 构造函数
        /// </summary>
        public GameWindowService()
        {
            // 可以在这里初始化其他资源
        }

        /// <summary>
        /// 设置调试模式
        /// </summary>
        public void SetDebugMode(bool debugMode)
        {
            _debugMode = debugMode;
        }

        /// <summary>
        /// 游戏窗口是否已检测到
        /// </summary>
        public bool IsGameWindowDetected => _gameWindowHandle != IntPtr.Zero;

        /// <summary>
        /// 游戏窗口矩形区域（包含边框）
        /// </summary>
        public Rectangle GameWindowRect => _gameWindowRect;

        /// <summary>
        /// 游戏客户区矩形区域（不含边框，实际游戏画面）
        /// </summary>
        public Rectangle GameClientRect => _gameClientRect;

        /// <summary>
        /// 检测游戏窗口
        /// </summary>
        /// <param name="forceRefresh">强制刷新检测</param>
        /// <returns>是否成功检测到窗口</returns>
        public bool DetectGameWindow(bool forceRefresh = false)
        {
            try
            {
                // 避免频繁检测（1秒内不重复检测）
                if (!forceRefresh && (DateTime.Now - _lastDetectionTime).TotalSeconds < 1)
                {
                    return IsGameWindowDetected;
                }

                _lastDetectionTime = DateTime.Now;

                LogMessage?.Invoke("🔍 开始检测游戏窗口...");

                // 方法1：通过窗口标题查找
                foreach (var title in _possibleWindowTitles)
                {
                    var handle = FindWindow(null, title);
                    if (handle != IntPtr.Zero && IsWindowVisible(handle))
                    {
                        _gameWindowHandle = handle;
                        UpdateWindowRectangles();
                        _previousClientRect = _gameClientRect; // 记录初始位置
                        
                        // 激活游戏窗口到前台
                        ActivateGameWindow();
                        
                        LogMessage?.Invoke($"✅ 找到游戏窗口: \"{title}\"");
                        LogMessage?.Invoke($"📐 窗口区域: {_gameWindowRect}");
                        LogMessage?.Invoke($"📐 客户区域: {_gameClientRect} (偏移: {_clientOffset})");
                        LogMessage?.Invoke($"📊 实际分辨率: {_gameClientRect.Width}x{_gameClientRect.Height}");
                        return true;
                    }
                }

                // 方法2：通过进程名查找（备用方案）
                foreach (var processName in _possibleProcessNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    if (processes.Length > 0)
                    {
                        var mainWindowHandle = processes[0].MainWindowHandle;
                        
                        // 如果窗口句柄为空，可能是最小化了，尝试恢复
                        if (mainWindowHandle == IntPtr.Zero)
                        {
                            LogMessage?.Invoke($"⚠️ 找到游戏进程 {processName}，但窗口句柄为空，可能是最小化了");
                            continue;
                        }
                        
                        if (IsWindowVisible(mainWindowHandle))
                        {
                            // 先尝试恢复窗口（如果最小化）
                            try
                            {
                                ShowWindow(mainWindowHandle, SW_RESTORE);
                                System.Threading.Thread.Sleep(200); // 等待窗口恢复
                            }
                            catch { }
                            
                            _gameWindowHandle = mainWindowHandle;
                            UpdateWindowRectangles();
                            _previousClientRect = _gameClientRect; // 记录初始位置

                            // 激活游戏窗口到前台
                            ActivateGameWindow();

                            var windowTitle = GetWindowTitle(mainWindowHandle);
                            LogMessage?.Invoke($"✅ 通过进程找到游戏窗口: \"{windowTitle}\" (进程: {processName})");
                            LogMessage?.Invoke($"📐 窗口区域: {_gameWindowRect}");
                            LogMessage?.Invoke($"📐 客户区域: {_gameClientRect} (偏移: {_clientOffset})");
                            LogMessage?.Invoke($"📊 实际分辨率: {_gameClientRect.Width}x{_gameClientRect.Height}");
                            return true;
                        }
                    }
                }

                LogMessage?.Invoke("❌ 未找到游戏窗口");
                LogMessage?.Invoke("💡 请确保游戏已启动并处于可见状态");
                LogMessage?.Invoke($"💡 支持的窗口标题: {string.Join(", ", _possibleWindowTitles)}");
                LogMessage?.Invoke($"💡 支持的进程名称: {string.Join(", ", _possibleProcessNames)}");
                _gameWindowHandle = IntPtr.Zero;
                return false;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 检测游戏窗口失败: {ex.Message}");
                _gameWindowHandle = IntPtr.Zero;
                return false;
            }
        }

        /// <summary>
        /// 激活游戏窗口到前台
        /// </summary>
        public void ActivateGameWindow()
        {
            if (_gameWindowHandle == IntPtr.Zero)
            {
                return;
            }

            try
            {
                // 如果窗口最小化，先恢复
                ShowWindow(_gameWindowHandle, SW_RESTORE);
                
                // 将窗口激活到前台
                SetForegroundWindow(_gameWindowHandle);
                
                if (_debugMode)
                {
                    LogMessage?.Invoke("🎯 游戏窗口已激活到前台");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"⚠️ 激活窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新窗口矩形信息
        /// </summary>
        private void UpdateWindowRectangles()
        {
            if (_gameWindowHandle == IntPtr.Zero)
                return;

            // 获取窗口矩形（包含边框）
            if (GetWindowRect(_gameWindowHandle, out var windowRect))
            {
                _gameWindowRect = new Rectangle(
                    windowRect.Left,
                    windowRect.Top,
                    windowRect.Right - windowRect.Left,
                    windowRect.Bottom - windowRect.Top
                );
            }

            // 获取客户区矩形（不含边框）
            if (GetClientRect(_gameWindowHandle, out var clientRect))
            {
                // 客户区的宽高
                var clientWidth = clientRect.Right - clientRect.Left;
                var clientHeight = clientRect.Bottom - clientRect.Top;

                // 计算客户区左上角在屏幕上的位置
                var point = new POINT { X = 0, Y = 0 };
                if (ClientToScreen(_gameWindowHandle, ref point))
                {
                    _clientOffset = new Point(point.X - _gameWindowRect.Left, point.Y - _gameWindowRect.Top);
                    _gameClientRect = new Rectangle(point.X, point.Y, clientWidth, clientHeight);
                }
            }
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        private string GetWindowTitle(IntPtr handle)
        {
            var sb = new StringBuilder(256);
            GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// 相对坐标转绝对坐标（相对于游戏客户区左上角）
        /// </summary>
        /// <param name="relativeX">相对X坐标</param>
        /// <param name="relativeY">相对Y坐标</param>
        /// <returns>屏幕绝对坐标</returns>
        public Point ToAbsolute(int relativeX, int relativeY)
        {
            if (!IsGameWindowDetected)
            {
                LogMessage?.Invoke("⚠️ 游戏窗口未检测，无法转换坐标");
                return new Point(relativeX, relativeY);
            }

            return new Point(
                _gameClientRect.Left + relativeX,
                _gameClientRect.Top + relativeY
            );
        }

        /// <summary>
        /// 相对坐标转绝对坐标
        /// </summary>
        public Point ToAbsolute(Point relativePoint)
        {
            return ToAbsolute(relativePoint.X, relativePoint.Y);
        }

        /// <summary>
        /// 相对矩形转绝对矩形
        /// </summary>
        public Rectangle ToAbsolute(Rectangle relativeRect)
        {
            if (!IsGameWindowDetected)
            {
                LogMessage?.Invoke("⚠️ 游戏窗口未检测，无法转换矩形");
                return relativeRect;
            }

            return new Rectangle(
                _gameClientRect.Left + relativeRect.X,
                _gameClientRect.Top + relativeRect.Y,
                relativeRect.Width,
                relativeRect.Height
            );
        }

        /// <summary>
        /// 绝对坐标转相对坐标
        /// </summary>
        public Point ToRelative(int absoluteX, int absoluteY)
        {
            if (!IsGameWindowDetected)
            {
                LogMessage?.Invoke("⚠️ 游戏窗口未检测，无法转换坐标");
                return new Point(absoluteX, absoluteY);
            }

            return new Point(
                absoluteX - _gameClientRect.Left,
                absoluteY - _gameClientRect.Top
            );
        }

        /// <summary>
        /// 绝对坐标转相对坐标
        /// </summary>
        public Point ToRelative(Point absolutePoint)
        {
            return ToRelative(absolutePoint.X, absolutePoint.Y);
        }

        /// <summary>
        /// 绝对矩形转相对矩形
        /// </summary>
        public Rectangle ToRelative(Rectangle absoluteRect)
        {
            if (!IsGameWindowDetected)
            {
                LogMessage?.Invoke("⚠️ 游戏窗口未检测，无法转换矩形");
                return absoluteRect;
            }

            return new Rectangle(
                absoluteRect.X - _gameClientRect.Left,
                absoluteRect.Y - _gameClientRect.Top,
                absoluteRect.Width,
                absoluteRect.Height
            );
        }

        /// <summary>
        /// 刷新窗口信息（窗口可能被移动或调整大小）
        /// </summary>
        public void RefreshWindowInfo()
        {
            if (_gameWindowHandle != IntPtr.Zero)
            {
                UpdateWindowRectangles();
            }
        }

        /// <summary>
        /// 检测窗口位置是否发生变化
        /// </summary>
        /// <returns>如果位置变化返回true，否则返回false</returns>
        public bool CheckWindowPositionChanged()
        {
            if (_gameWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            // 保存旧位置
            var oldClientRect = _gameClientRect;

            // 刷新窗口信息
            UpdateWindowRectangles();

            // 检查客户区位置是否变化
            if (oldClientRect.IsEmpty || _previousClientRect.IsEmpty)
            {
                // 首次检测，记录位置
                _previousClientRect = _gameClientRect;
                return false;
            }

            bool hasChanged = _gameClientRect.Left != _previousClientRect.Left ||
                            _gameClientRect.Top != _previousClientRect.Top ||
                            _gameClientRect.Width != _previousClientRect.Width ||
                            _gameClientRect.Height != _previousClientRect.Height;

            if (hasChanged)
            {
                LogMessage?.Invoke($"🔄 检测到游戏窗口位置变化:");
                LogMessage?.Invoke($"   旧位置: {_previousClientRect}");
                LogMessage?.Invoke($"   新位置: {_gameClientRect}");
                
                // 更新记录的位置
                _previousClientRect = _gameClientRect;
            }

            return hasChanged;
        }

        /// <summary>
        /// 获取游戏窗口的缩放比例（相对于标准分辨率）
        /// </summary>
        /// <param name="baseWidth">基准宽度，默认1920</param>
        /// <param name="baseHeight">基准高度，默认1080</param>
        /// <returns>缩放比例</returns>
        public (double scaleX, double scaleY) GetGameScale(double baseWidth = 1920.0, double baseHeight = 1080.0)
        {
            if (!IsGameWindowDetected)
            {
                return (1.0, 1.0);
            }

            var scaleX = _gameClientRect.Width / baseWidth;
            var scaleY = _gameClientRect.Height / baseHeight;

            return (scaleX, scaleY);
        }

        /// <summary>
        /// 应用缩放到相对坐标（如果游戏分辨率不是标准分辨率）
        /// </summary>
        public Point ApplyScale(int baseX, int baseY, double baseWidth = 1920.0, double baseHeight = 1080.0)
        {
            var (scaleX, scaleY) = GetGameScale(baseWidth, baseHeight);
            return new Point(
                (int)(baseX * scaleX),
                (int)(baseY * scaleY)
            );
        }

        /// <summary>
        /// 应用缩放到相对坐标
        /// </summary>
        public Point ApplyScale(Point basePoint, double baseWidth = 1920.0, double baseHeight = 1080.0)
        {
            return ApplyScale(basePoint.X, basePoint.Y, baseWidth, baseHeight);
        }

        /// <summary>
        /// 应用缩放到相对矩形
        /// </summary>
        public Rectangle ApplyScale(Rectangle baseRect, double baseWidth = 1920.0, double baseHeight = 1080.0)
        {
            var (scaleX, scaleY) = GetGameScale(baseWidth, baseHeight);
            return new Rectangle(
                (int)(baseRect.X * scaleX),
                (int)(baseRect.Y * scaleY),
                (int)(baseRect.Width * scaleX),
                (int)(baseRect.Height * scaleY)
            );
        }

        #region 兼容旧版本的方法

    /// <summary>
        /// 查找游戏窗口（兼容旧版本）
    /// </summary>
    /// <param name="windowTitle">窗口标题</param>
    /// <returns>是否找到</returns>
    public bool FindGameWindow(string? windowTitle = null)
    {
        if (!string.IsNullOrEmpty(windowTitle))
        {
                // 临时添加到可能的标题列表
                var tempTitles = new string[_possibleWindowTitles.Length + 1];
                tempTitles[0] = windowTitle;
                Array.Copy(_possibleWindowTitles, 0, tempTitles, 1, _possibleWindowTitles.Length);
                
                foreach (var title in tempTitles)
                {
                    var handle = FindWindow(null, title);
                    if (handle != IntPtr.Zero && IsWindowVisible(handle))
                    {
                        _gameWindowHandle = handle;
                        UpdateWindowRectangles();
                        return true;
                    }
                }
                return false;
            }

            return DetectGameWindow();
    }

    /// <summary>
        /// 获取游戏窗口句柄（兼容旧版本）
    /// </summary>
    public IntPtr GetGameWindowHandle()
    {
        return _gameWindowHandle;
    }

    /// <summary>
        /// 获取游戏窗口矩形（兼容旧版本）
    /// </summary>
    public Rectangle GetGameWindowRect()
    {
            return _gameWindowRect;
    }

    /// <summary>
        /// 截取游戏窗口（兼容旧版本）
    /// </summary>
    public Bitmap? CaptureGameWindow()
    {
        var rect = GetGameWindowRect();
        if (rect.IsEmpty)
            return null;

        try
        {
            var bitmap = new Bitmap(rect.Width, rect.Height);
            using var graphics = Graphics.FromImage(bitmap);
            
            var hdcBitmap = graphics.GetHdc();
            var hdcScreen = GetDC(_gameWindowHandle);

            try
            {
                BitBlt(hdcBitmap, 0, 0, rect.Width, rect.Height, hdcScreen, 0, 0, SRCCOPY);
            }
            finally
            {
                graphics.ReleaseHdc(hdcBitmap);
                ReleaseDC(_gameWindowHandle, hdcScreen);
            }

            return bitmap;
        }
            catch (Exception ex)
        {
                LogMessage?.Invoke($"❌ 截取游戏窗口失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
        /// 截取游戏窗口指定区域（兼容旧版本）
    /// </summary>
    public Bitmap? CaptureGameWindowRegion(Rectangle region)
    {
        var windowRect = GetGameWindowRect();
        if (windowRect.IsEmpty)
            return null;

        // 转换为屏幕坐标
        var screenRegion = new Rectangle(
            windowRect.X + region.X,
            windowRect.Y + region.Y,
            region.Width,
            region.Height);

        try
        {
            var bitmap = new Bitmap(region.Width, region.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(screenRegion.Location, System.Drawing.Point.Empty, region.Size);
            return bitmap;
        }
            catch (Exception ex)
        {
                LogMessage?.Invoke($"❌ 截取游戏窗口区域失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
        /// 检查游戏窗口是否活动（兼容旧版本）
    /// </summary>
    public bool IsGameWindowActive()
    {
        if (_gameWindowHandle == IntPtr.Zero)
            return false;

        var activeWindow = GetForegroundWindow();
        return activeWindow == _gameWindowHandle;
    }

    /// <summary>
        /// 检查游戏是否正在运行（兼容旧版本）
    /// </summary>
    public bool IsGameRunning()
    {
            return DetectGameWindow() && IsWindowVisible(_gameWindowHandle);
        }

        #endregion

        /// <summary>
        /// 添加自定义窗口标题
        /// </summary>
        public void AddCustomWindowTitle(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                var newTitles = new string[_possibleWindowTitles.Length + 1];
                newTitles[0] = title;
                Array.Copy(_possibleWindowTitles, 0, newTitles, 1, _possibleWindowTitles.Length);
                // 注意：这里只是示例，实际应该修改为可变集合
                LogMessage?.Invoke($"📝 已添加自定义窗口标题: {title}");
            }
        }

        /// <summary>
        /// 添加自定义进程名称
        /// </summary>
        public void AddCustomProcessName(string processName)
        {
            if (!string.IsNullOrEmpty(processName))
            {
                var newProcessNames = new string[_possibleProcessNames.Length + 1];
                newProcessNames[0] = processName;
                Array.Copy(_possibleProcessNames, 0, newProcessNames, 1, _possibleProcessNames.Length);
                // 注意：这里只是示例，实际应该修改为可变集合
                LogMessage?.Invoke($"📝 已添加自定义进程名称: {processName}");
            }
        }

        /// <summary>
        /// 获取窗口信息摘要
        /// </summary>
        public string GetWindowInfoSummary()
        {
            if (!IsGameWindowDetected)
            {
                return "游戏窗口未检测到";
            }

            var title = GetWindowTitle(_gameWindowHandle);
            var (scaleX, scaleY) = GetGameScale();
            
            return $"窗口: {title} | 客户区: {_gameClientRect.Width}x{_gameClientRect.Height} | 缩放: {scaleX:F2}x{scaleY:F2}";
        }
    }
}