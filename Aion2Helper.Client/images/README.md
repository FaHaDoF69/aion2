# 图像模板文件说明

此目录用于存放图像识别所需的模板文件。ImageDetectionService 提供强大的通用图像识别功能，参考大漠工具的设计理念。

## 使用方式

### 1. 准备模板图像
- 将需要识别的图像保存到此目录
- 支持常见图像格式：JPG、PNG、BMP等
- 建议使用清晰、特征明显的图像作为模板

### 2. 代码中使用
```csharp
// 初始化服务
var imageService = new ImageDetectionService(gameWindowService);
imageService.SetDebugMode(true);

// 基础模板匹配
var template = imageService.LoadTemplate("images/button.jpg");
var screenshot = imageService.CaptureGameWindow();
var result = imageService.MatchTemplate(screenshot, template, 0.8);

// 颜色识别
var redPoints = imageService.FindColorPoints(screenshot, Color.Red, 10);
var pixelColor = imageService.GetPixelColor(screenshot, new Point(100, 100));

// 图像预处理
var grayImage = imageService.ConvertToGrayscale(screenshot);
var binaryImage = imageService.ConvertToBinary(grayImage, 127);
var blurredImage = imageService.ApplyGaussianBlur(screenshot, 5);

// 图像相似度比较
var similarity = imageService.CompareImageSimilarity(image1, image2);
```

## 完整功能列表

### 🖼️ **截图功能**
- `CaptureGameWindow()` - 截取游戏窗口
- `CaptureScreenArea(Rectangle area)` - 截取屏幕指定区域

### 🔍 **模板匹配**
- `LoadTemplate(string path)` - 加载模板图像
- `MatchTemplate()` - 单个模板匹配，返回最佳匹配
- `MatchTemplateMultiple()` - 多个模板匹配，返回所有匹配项

### 🎨 **颜色识别**（参考大漠工具）
- `FindColorPoints()` - 查找指定颜色的所有像素点
- `FindFirstColorPoint()` - 查找第一个匹配的颜色点
- `GetPixelColor()` - 获取指定位置的像素颜色

### 📊 **图像比较**
- `CompareImageSimilarity()` - 计算两个图像的相似度

### 🔄 **图像预处理**
- `ConvertToGrayscale()` - 转换为灰度图
- `ConvertToBinary()` - 二值化处理
- `ApplyGaussianBlur()` - 高斯模糊降噪
- `DetectEdges()` - Canny边缘检测

### 📐 **坐标工具**
- `RelativeToAbsolute()` - 相对坐标转绝对坐标
- `AbsoluteToRelative()` - 绝对坐标转相对坐标
- `IsPointInRectangle()` - 检查点是否在矩形内
- `CalculateDistance()` - 计算两点距离

### ✏️ **调试绘制**
- `DrawRectangle()` - 在图像上绘制矩形框
- `DrawPoint()` - 在图像上绘制点
- `SaveImage()` - 保存图像到文件

## 图像要求

1. **格式**: 支持 JPG、PNG、BMP 等常见格式
2. **质量**: 清晰、无模糊、特征明显
3. **尺寸**: 建议不要过大，一般 20x20 到 200x200 像素
4. **背景**: 尽量包含完整的特征区域
5. **颜色**: 保持原始游戏颜色，避免过度处理

## 高级用法示例

### 颜色识别示例
```csharp
// 查找红色按钮
var redColor = Color.FromArgb(255, 0, 0);
var redPoints = imageService.FindColorPoints(screenshot, redColor, tolerance: 20);

// 检查特定位置的颜色
var color = imageService.GetPixelColor(screenshot, new Point(500, 300));
if (color?.R > 200) // 红色分量较高
{
    Console.WriteLine("检测到红色区域");
}
```

### 图像预处理示例
```csharp
// 图像预处理提高识别率
var processed = imageService.ConvertToGrayscale(screenshot);
processed = imageService.ApplyGaussianBlur(processed, 3);
processed = imageService.ConvertToBinary(processed, 128);

// 使用处理后的图像进行模板匹配
var result = imageService.MatchTemplate(processed, template, 0.7);
```

### 多区域搜索示例
```csharp
// 在不同区域搜索相同模板
var areas = new[] {
    new Rectangle(0, 0, 400, 300),      // 左上区域
    new Rectangle(400, 0, 400, 300),    // 右上区域
    new Rectangle(0, 300, 400, 300)     // 左下区域
};

foreach (var area in areas)
{
    var results = imageService.MatchTemplateMultiple(screenshot, template, 0.8, area);
    Console.WriteLine($"区域 {area} 找到 {results.Count} 个匹配项");
}
```

## 调试提示

- 启用调试模式可以查看详细的处理过程
- 使用 `SaveImage()` 保存中间处理结果进行分析
- 颜色识别时注意光照和显示器色彩差异
- 模板匹配失败时可以尝试图像预处理
- 使用绘制功能标记检测结果便于调试