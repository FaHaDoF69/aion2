using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 图像识别服务 - 提供通用的图像识别和模板匹配功能（支持异步处理）
    /// </summary>
    public class ImageDetectionService
    {
        private bool _debugMode = false;
        public event Action<string>? LogMessage;
        private readonly GameWindowService _gameWindowService;
        private readonly OCRService _ocrService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ImageDetectionService(GameWindowService gameWindowService)
        {
            _gameWindowService = gameWindowService;
            _ocrService = new OCRService();
            _ocrService.LogMessage += message => LogMessage?.Invoke(message);
        }

        /// <summary>
        /// 设置调试模式
        /// </summary>
        public void SetDebugMode(bool debugMode)
        {
            _debugMode = debugMode;
            _ocrService.SetDebugMode(debugMode);
        }

        #region 基础截图功能

        /// <summary>
        /// 截取游戏窗口截图
        /// </summary>
        public Mat? CaptureGameWindow()
        {
            try
            {
                var gameRect = _gameWindowService.GetGameWindowRect();
                if (gameRect == Rectangle.Empty)
                {
                    LogMessage?.Invoke("❌ 无法获取游戏窗口位置");
                    return null;
                }

                using var bitmap = new Bitmap(gameRect.Width, gameRect.Height);
                using var graphics = Graphics.FromImage(bitmap);
                
                graphics.CopyFromScreen(gameRect.Left, gameRect.Top, 0, 0, 
                    new System.Drawing.Size(gameRect.Width, gameRect.Height));

                var mat = BitmapConverter.ToMat(bitmap);
                
                if (_debugMode)
                {
                    LogMessage?.Invoke($"📸 截取游戏窗口: {gameRect.Width}x{gameRect.Height}");
                }
                
                return mat;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 截取游戏窗口失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 截取屏幕指定区域
        /// </summary>
        public Mat? CaptureScreenArea(Rectangle area)
        {
            try
            {
                using var bitmap = new Bitmap(area.Width, area.Height);
                using var graphics = Graphics.FromImage(bitmap);
                
                graphics.CopyFromScreen(area.Left, area.Top, 0, 0, 
                    new System.Drawing.Size(area.Width, area.Height));

                var mat = BitmapConverter.ToMat(bitmap);
                
                if (_debugMode)
                {
                    LogMessage?.Invoke($"📸 截取屏幕区域: ({area.X},{area.Y}) 大小:{area.Width}x{area.Height}");
                }
                
                return mat;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 截取屏幕区域失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载模板图像
        /// </summary>
        public Mat? LoadTemplate(string templatePath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    LogMessage?.Invoke($"❌ 模板文件不存在: {templatePath}");
                    return null;
                }

                var template = Cv2.ImRead(templatePath, ImreadModes.Color);
                
                if (template == null || template.Empty())
                {
                    LogMessage?.Invoke($"❌ 模板加载失败: {templatePath}");
                    return null;
                }

                if (_debugMode)
                {
                    LogMessage?.Invoke($"✅ 模板加载成功: {templatePath} ({template.Width}x{template.Height})");
                }
                
                return template;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 加载模板异常: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 模板匹配功能

        /// <summary>
        /// 单个模板匹配
        /// </summary>
        public TemplateMatchResult? MatchTemplate(Mat sourceImage, Mat template, double threshold = 0.7, Rectangle? searchArea = null)
        {
            try
            {
                if (sourceImage == null || sourceImage.Empty())
                {
                    LogMessage?.Invoke("❌ 源图像为空");
                    return null;
                }

                if (template == null || template.Empty())
                {
                    LogMessage?.Invoke("❌ 模板图像为空");
                    return null;
                }

                Mat searchRegion = sourceImage;
                int offsetX = 0, offsetY = 0;

                if (searchArea.HasValue)
                {
                    var area = searchArea.Value;
                    if (area.X >= 0 && area.Y >= 0 && 
                        area.X + area.Width <= sourceImage.Width && 
                        area.Y + area.Height <= sourceImage.Height)
                    {
                        searchRegion = new Mat(sourceImage, new OpenCvSharp.Rect(area.X, area.Y, area.Width, area.Height));
                        offsetX = area.X;
                        offsetY = area.Y;
                        
                        if (_debugMode)
                        {
                            LogMessage?.Invoke($"🔍 在指定区域搜索: ({area.X},{area.Y}) 大小:{area.Width}x{area.Height}");
                        }
                    }
                }

                using var result = new Mat();
                Cv2.MatchTemplate(searchRegion, template, result, TemplateMatchModes.CCoeffNormed);

                double minVal, maxVal;
                OpenCvSharp.Point minLoc, maxLoc;
                Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

                if (_debugMode)
                {
                    LogMessage?.Invoke($"🔍 模板匹配结果 - 最高相似度: {maxVal:P1}, 阈值: {threshold:P1}");
                }

                if (maxVal < threshold)
                {
                    if (_debugMode)
                    {
                        LogMessage?.Invoke($"❌ 未找到匹配项（相似度{maxVal:P1} < {threshold:P1}）");
                    }
                    
                    if (searchArea.HasValue && searchRegion != sourceImage)
                    {
                        searchRegion.Dispose();
                    }
                    
                    return null;
                }

                var matchPoint = new System.Drawing.Point(
                    offsetX + maxLoc.X,
                    offsetY + maxLoc.Y
                );

                var centerPoint = new System.Drawing.Point(
                    matchPoint.X + template.Width / 2,
                    matchPoint.Y + template.Height / 2
                );

                var matchResult = new TemplateMatchResult
                {
                    TopLeft = matchPoint,
                    Center = centerPoint,
                    Confidence = maxVal,
                    TemplateSize = new System.Drawing.Size(template.Width, template.Height)
                };

                if (_debugMode)
                {
                    LogMessage?.Invoke($"✅ 找到匹配项: 左上角({matchPoint.X},{matchPoint.Y}) 中心({centerPoint.X},{centerPoint.Y}) 相似度:{maxVal:P1}");
                }

                if (searchArea.HasValue && searchRegion != sourceImage)
                {
                    searchRegion.Dispose();
                }

                return matchResult;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 模板匹配异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 异步模板匹配
        /// </summary>
        public async Task<TemplateMatchResult?> MatchTemplateAsync(Mat sourceImage, Mat template, double threshold = 0.7, Rectangle? searchArea = null)
        {
            return await Task.Run(() => MatchTemplate(sourceImage, template, threshold, searchArea));
        }

        /// <summary>
        /// 异步批量模板匹配 - 多个模板并行处理
        /// </summary>
        public async Task<Dictionary<string, TemplateMatchResult?>> MatchMultipleTemplatesAsync(
            Mat sourceImage, 
            Dictionary<string, double> templates, 
            Rectangle? searchArea = null)
        {
            var tasks = templates.Select(async kvp =>
            {
                var templatePath = kvp.Key;
                var threshold = kvp.Value;
                
                return await Task.Run(() =>
                {
                    using var template = LoadTemplate(templatePath);
                    if (template == null) return new KeyValuePair<string, TemplateMatchResult?>(templatePath, null);
                    
                    var result = MatchTemplate(sourceImage, template, threshold, searchArea);
                    return new KeyValuePair<string, TemplateMatchResult?>(templatePath, result);
                });
            });

            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(r => r.Key, r => r.Value);
        }

        /// <summary>
        /// 异步游戏窗口截图和模板匹配组合操作
        /// </summary>
        public async Task<TemplateMatchResult?> CaptureAndMatchAsync(string templatePath, double threshold = 0.7, Rectangle? searchArea = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var screenshot = CaptureGameWindow();
                    if (screenshot == null) return null;

                    using var template = LoadTemplate(templatePath);
                    if (template == null) return null;

                    return MatchTemplate(screenshot, template, threshold, searchArea);
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"❌ 异步截图匹配异常: {ex.Message}");
                    return null;
                }
            });
        }

        #endregion

        #region 颜色识别功能

        /// <summary>
        /// 颜色识别 - 在指定区域查找指定颜色的像素点
        /// </summary>
        public List<System.Drawing.Point> FindColorPoints(Mat sourceImage, Color targetColor, int tolerance = 10, Rectangle? searchArea = null)
        {
            var colorPoints = new List<System.Drawing.Point>();
            
            try
            {
                if (sourceImage == null || sourceImage.Empty())
                {
                    LogMessage?.Invoke("❌ 源图像为空");
                    return colorPoints;
                }

                Mat searchRegion = sourceImage;
                int offsetX = 0, offsetY = 0;

                if (searchArea.HasValue)
                {
                    var area = searchArea.Value;
                    if (area.X >= 0 && area.Y >= 0 && 
                        area.X + area.Width <= sourceImage.Width && 
                        area.Y + area.Height <= sourceImage.Height)
                    {
                        searchRegion = new Mat(sourceImage, new OpenCvSharp.Rect(area.X, area.Y, area.Width, area.Height));
                        offsetX = area.X;
                        offsetY = area.Y;
                    }
                }

                var lowerBound = new Scalar(
                    Math.Max(0, targetColor.B - tolerance),
                    Math.Max(0, targetColor.G - tolerance),
                    Math.Max(0, targetColor.R - tolerance)
                );
                var upperBound = new Scalar(
                    Math.Min(255, targetColor.B + tolerance),
                    Math.Min(255, targetColor.G + tolerance),
                    Math.Min(255, targetColor.R + tolerance)
                );

                using var mask = new Mat();
                Cv2.InRange(searchRegion, lowerBound, upperBound, mask);

                for (int y = 0; y < mask.Height; y++)
                {
                    for (int x = 0; x < mask.Width; x++)
                    {
                        if (mask.At<byte>(y, x) > 0)
                        {
                            colorPoints.Add(new System.Drawing.Point(offsetX + x, offsetY + y));
                        }
                    }
                }

                if (_debugMode)
                {
                    LogMessage?.Invoke($"🎨 找到 {colorPoints.Count} 个颜色匹配点 RGB({targetColor.R},{targetColor.G},{targetColor.B}) 容差:{tolerance}");
                }

                if (searchArea.HasValue && searchRegion != sourceImage)
                {
                    searchRegion.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 颜色识别异常: {ex.Message}");
            }
            
            return colorPoints;
        }

        /// <summary>
        /// 异步颜色识别
        /// </summary>
        public async Task<List<System.Drawing.Point>> FindColorPointsAsync(Mat sourceImage, Color targetColor, int tolerance = 10, Rectangle? searchArea = null)
        {
            return await Task.Run(() => FindColorPoints(sourceImage, targetColor, tolerance, searchArea));
        }

        #endregion

        #region OCR文字识别功能

        /// <summary>
        /// 从图像中识别文字
        /// </summary>
        public OCRResult? RecognizeText(Mat image, bool preprocessImage = true)
        {
            return _ocrService.RecognizeText(image, preprocessImage);
        }

        /// <summary>
        /// 从指定区域识别文字
        /// </summary>
        public OCRResult? RecognizeTextInRegion(Mat image, Rectangle region, bool preprocessImage = true)
        {
            return _ocrService.RecognizeTextInRegion(image, region, preprocessImage);
        }

        /// <summary>
        /// 识别数字
        /// </summary>
        public int? RecognizeNumber(Mat image, bool preprocessImage = true)
        {
            return _ocrService.RecognizeNumber(image, preprocessImage);
        }

        /// <summary>
        /// 检查OCR引擎是否可用
        /// </summary>
        public bool IsOCRAvailable()
        {
            return _ocrService.IsEngineAvailable();
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 保存图像到文件（调试用）
        /// </summary>
        public void SaveImage(Mat image, string filePath)
        {
            try
            {
                if (image == null || image.Empty())
                {
                    LogMessage?.Invoke("❌ 图像为空，无法保存");
                    return;
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Cv2.ImWrite(filePath, image);
                
                if (_debugMode)
                {
                    LogMessage?.Invoke($"💾 图像已保存: {filePath}");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 保存图像失败: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _ocrService?.Dispose();
        }
    }

    /// <summary>
    /// 模板匹配结果
    /// </summary>
    public class TemplateMatchResult
    {
        public System.Drawing.Point TopLeft { get; set; }
        public System.Drawing.Point Center { get; set; }
        public double Confidence { get; set; }
        public System.Drawing.Size TemplateSize { get; set; }
        public Rectangle MatchRectangle => new Rectangle(TopLeft, TemplateSize);
    }
}

