# GameWindowService 使用说明

GameWindowService 是一个功能强大的游戏窗口检测和坐标转换服务，参考了先进的窗口定位技术。

## 主要功能

### 🔍 **智能窗口检测**
- 支持多种窗口标题自动检测
- 支持进程名称检测（备用方案）
- 自动处理最小化窗口恢复
- 窗口可见性验证

### 📐 **坐标转换系统**
- 相对坐标 ↔ 绝对坐标转换
- 客户区坐标精确计算
- 窗口边框自动处理
- 支持矩形区域转换

### 📊 **缩放适配**
- 自动检测窗口分辨率
- 基于基准分辨率的缩放计算
- 支持自定义基准分辨率
- 坐标和矩形缩放适配

### 🔄 **窗口监控**
- 实时窗口位置变化检测
- 窗口大小调整监控
- 自动刷新窗口信息
- 窗口状态跟踪

## 基础使用

### 1. 初始化服务
```csharp
var windowService = new GameWindowService();
windowService.SetDebugMode(true); // 启用调试日志
windowService.LogMessage += message => Console.WriteLine(message);
```

### 2. 检测游戏窗口
```csharp
// 自动检测（推荐）
bool detected = windowService.DetectGameWindow();

// 强制刷新检测
bool detected = windowService.DetectGameWindow(forceRefresh: true);

// 兼容旧版本方式
bool found = windowService.FindGameWindow("Aion2");
```

### 3. 获取窗口信息
```csharp
if (windowService.IsGameWindowDetected)
{
    var windowRect = windowService.GameWindowRect;     // 包含边框的窗口区域
    var clientRect = windowService.GameClientRect;     // 客户区域（实际游戏画面）
    
    Console.WriteLine($"窗口区域: {windowRect}");
    Console.WriteLine($"客户区域: {clientRect}");
    Console.WriteLine($"窗口信息: {windowService.GetWindowInfoSummary()}");
}
```

## 坐标转换

### 相对坐标转绝对坐标
```csharp
// 游戏内坐标 (100, 200) 转换为屏幕坐标
var absolutePoint = windowService.ToAbsolute(100, 200);
var absolutePoint2 = windowService.ToAbsolute(new Point(100, 200));

// 矩形区域转换
var gameRect = new Rectangle(50, 50, 200, 100);
var screenRect = windowService.ToAbsolute(gameRect);
```

### 绝对坐标转相对坐标
```csharp
// 屏幕坐标转换为游戏内坐标
var relativePoint = windowService.ToRelative(screenX, screenY);
var relativePoint2 = windowService.ToRelative(new Point(screenX, screenY));

// 矩形区域转换
var screenRect = new Rectangle(500, 300, 200, 100);
var gameRect = windowService.ToRelative(screenRect);
```

## 缩放适配

### 获取缩放比例
```csharp
// 基于默认 1920x1080 分辨率
var (scaleX, scaleY) = windowService.GetGameScale();

// 基于自定义分辨率
var (scaleX2, scaleY2) = windowService.GetGameScale(1366, 768);

Console.WriteLine($"缩放比例: {scaleX:F2} x {scaleY:F2}");
```

### 应用缩放
```csharp
// 将基于 1920x1080 的坐标适配到当前分辨率
var scaledPoint = windowService.ApplyScale(960, 540); // 屏幕中心点

// 基于自定义基准分辨率
var scaledPoint2 = windowService.ApplyScale(683, 384, 1366, 768);

// 矩形缩放
var baseRect = new Rectangle(100, 100, 200, 150);
var scaledRect = windowService.ApplyScale(baseRect);
```

## 窗口监控

### 检测窗口变化
```csharp
// 定期检查窗口位置是否变化
Timer timer = new Timer(1000); // 每秒检查一次
timer.Elapsed += (sender, e) => 
{
    if (windowService.CheckWindowPositionChanged())
    {
        Console.WriteLine("窗口位置发生变化，需要更新坐标！");
        // 重新计算相关坐标...
    }
};
timer.Start();
```

### 手动刷新窗口信息
```csharp
// 当怀疑窗口信息过时时
windowService.RefreshWindowInfo();
```

## 高级功能

### 自定义窗口标题和进程名
```csharp
// 添加自定义窗口标题
windowService.AddCustomWindowTitle("我的游戏窗口");

// 添加自定义进程名称
windowService.AddCustomProcessName("MyGame");
```

### 窗口激活
```csharp
// 激活游戏窗口到前台
windowService.ActivateGameWindow();

// 检查窗口是否处于活动状态
bool isActive = windowService.IsGameWindowActive();
```

### 截图功能（兼容旧版本）
```csharp
// 截取整个游戏窗口
using var screenshot = windowService.CaptureGameWindow();

// 截取游戏窗口指定区域
var region = new Rectangle(100, 100, 300, 200);
using var regionScreenshot = windowService.CaptureGameWindowRegion(region);
```

## 完整示例

```csharp
public class GameAutomation
{
    private GameWindowService _windowService;
    
    public GameAutomation()
    {
        _windowService = new GameWindowService();
        _windowService.SetDebugMode(true);
        _windowService.LogMessage += Console.WriteLine;
    }
    
    public async Task<bool> InitializeAsync()
    {
        // 尝试检测游戏窗口
        if (!_windowService.DetectGameWindow())
        {
            Console.WriteLine("未找到游戏窗口，请启动游戏后重试");
            return false;
        }
        
        // 激活游戏窗口
        _windowService.ActivateGameWindow();
        
        // 显示窗口信息
        Console.WriteLine(_windowService.GetWindowInfoSummary());
        
        return true;
    }
    
    public void ClickGameButton()
    {
        // 假设按钮在游戏内坐标 (500, 300)
        var gameButtonPos = new Point(500, 300);
        
        // 应用缩放适配（如果游戏分辨率不是 1920x1080）
        var scaledPos = _windowService.ApplyScale(gameButtonPos);
        
        // 转换为屏幕绝对坐标
        var screenPos = _windowService.ToAbsolute(scaledPos);
        
        // 执行点击操作
        Console.WriteLine($"点击屏幕坐标: {screenPos}");
        // MouseClick(screenPos.X, screenPos.Y);
    }
    
    public void MonitorWindowChanges()
    {
        var timer = new System.Timers.Timer(2000); // 每2秒检查一次
        timer.Elapsed += (sender, e) =>
        {
            if (_windowService.CheckWindowPositionChanged())
            {
                Console.WriteLine("检测到窗口变化，重新计算坐标...");
                // 这里可以重新计算所有相关坐标
            }
        };
        timer.Start();
    }
}

// 使用示例
var automation = new GameAutomation();
if (await automation.InitializeAsync())
{
    automation.ClickGameButton();
    automation.MonitorWindowChanges();
}
```

## 支持的游戏窗口

### 默认支持的窗口标题
- "Aion2"
- "AION2" 
- "Aion 2"
- "永恒之塔2"
- "永恒之塔 2"

### 默认支持的进程名称
- "Aion2"
- "AION2"
- "aion2"
- "Aion"
- "AION"

## 注意事项

1. **权限要求**: 需要足够的权限访问其他进程的窗口信息
2. **性能优化**: 避免频繁调用检测方法，建议使用缓存机制
3. **异常处理**: 窗口可能随时关闭或最小化，需要适当的错误处理
4. **坐标精度**: 坐标转换涉及整数运算，可能存在1-2像素的误差
5. **多显示器**: 在多显示器环境下需要注意坐标系统的差异

## 调试技巧

1. **启用调试模式**: `SetDebugMode(true)` 查看详细日志
2. **监听日志事件**: 订阅 `LogMessage` 事件获取运行状态
3. **定期检查**: 使用 `GetWindowInfoSummary()` 获取当前状态
4. **强制刷新**: 遇到问题时使用 `DetectGameWindow(forceRefresh: true)`


