using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;

namespace Aion2Helper.Services
{
    /// <summary>
    /// OCR服务 - 光学字符识别功能
    /// </summary>
    public class OCRService : IDisposable
    {
        private bool _debugMode = false;
        public event Action<string>? LogMessage;
        
        private TesseractEngine? _engine;
        private readonly string _tessDataPath;
        private readonly object _lockObject = new();
        private bool _disposed = false;

        public OCRService()
        {
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            
            if (!Directory.Exists(_tessDataPath))
            {
                Directory.CreateDirectory(_tessDataPath);
                LogMessage?.Invoke($"📁 创建Tesseract数据目录: {_tessDataPath}");
                LogMessage?.Invoke("⚠️ 请下载并放置Tesseract语言数据文件到tessdata目录");
            }
            
            InitializeTesseract();
        }

        public void SetDebugMode(bool debugMode)
        {
            _debugMode = debugMode;
        }

        private void InitializeTesseract()
        {
            try
            {
                lock (_lockObject)
                {
                    var availableLanguages = GetAvailableLanguages();
                    
                    if (availableLanguages.Count == 0)
                    {
                        LogMessage?.Invoke("❌ 未找到Tesseract语言数据文件");
                        LogMessage?.Invoke("💡 请下载eng.traineddata和chi_sim.traineddata到tessdata目录");
                        return;
                    }

                    string language = "eng";
                    if (availableLanguages.Contains("chi_sim") && availableLanguages.Contains("eng"))
                    {
                        language = "chi_sim+eng";
                    }
                    else if (availableLanguages.Contains("chi_sim"))
                    {
                        language = "chi_sim";
                    }
                    else if (availableLanguages.Contains("eng"))
                    {
                        language = "eng";
                    }
                    else
                    {
                        language = availableLanguages.First();
                    }

                    _engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
                    _engine.SetVariable("tessedit_pageseg_mode", "6");
                    
                    LogMessage?.Invoke($"✅ Tesseract引擎初始化成功，语言: {language}");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ Tesseract引擎初始化失败: {ex.Message}");
            }
        }

        private List<string> GetAvailableLanguages()
        {
            var languages = new List<string>();
            
            try
            {
                if (Directory.Exists(_tessDataPath))
                {
                    var files = Directory.GetFiles(_tessDataPath, "*.traineddata");
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            languages.Add(fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"⚠️ 获取语言列表失败: {ex.Message}");
            }
            
            return languages;
        }

        public OCRResult? RecognizeText(Mat image, bool preprocessImage = true)
        {
            if (_engine == null)
            {
                LogMessage?.Invoke("❌ OCR引擎未初始化");
                return null;
            }

            try
            {
                lock (_lockObject)
                {
                    if (image == null || image.Empty())
                    {
                        LogMessage?.Invoke("❌ 输入图像为空");
                        return null;
                    }

                    Mat processedImage = image;
                    
                    if (preprocessImage)
                    {
                        processedImage = PreprocessImageForOCR(image);
                    }

                    var bitmap = BitmapConverter.ToBitmap(processedImage);
                    var tempFile = Path.Combine(Path.GetTempPath(), $"ocr_temp_{Guid.NewGuid()}.png");
                    
                    try
                    {
                        bitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        
                        var pix = Pix.LoadFromFile(tempFile);
                        var page = _engine.Process(pix);
                        
                        var text = page.GetText().Trim();
                        var confidence = page.GetMeanConfidence();
                        
                        var words = new List<OCRWord>();
                        var iterator = page.GetIterator();
                        
                        do
                        {
                            var word = iterator.GetText(PageIteratorLevel.Word);
                            if (!string.IsNullOrWhiteSpace(word))
                            {
                                iterator.TryGetBoundingBox(PageIteratorLevel.Word, out var rect);
                                var wordConfidence = iterator.GetConfidence(PageIteratorLevel.Word);
                                
                                words.Add(new OCRWord
                                {
                                    Text = word.Trim(),
                                    Confidence = wordConfidence,
                                    BoundingBox = new Rectangle(rect.X1, rect.Y1, rect.X2 - rect.X1, rect.Y2 - rect.Y1)
                                });
                            }
                        } while (iterator.Next(PageIteratorLevel.Word));

                        var result = new OCRResult
                        {
                            Text = text,
                            Confidence = confidence,
                            Words = words,
                            ProcessingTime = DateTime.Now
                        };

                        if (_debugMode)
                        {
                            LogMessage?.Invoke($"📝 OCR识别结果: \"{text}\" (置信度: {confidence:P1})");
                            LogMessage?.Invoke($"📊 识别到 {words.Count} 个单词");
                        }

                        iterator.Dispose();
                        page.Dispose();
                        pix.Dispose();
                        
                        if (preprocessImage && processedImage != image)
                        {
                            processedImage.Dispose();
                        }
                        
                        bitmap.Dispose();
                        
                        return result;
                    }
                    finally
                    {
                        try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ OCR识别异常: {ex.Message}");
                return null;
            }
        }

        public OCRResult? RecognizeTextInRegion(Mat image, Rectangle region, bool preprocessImage = true)
        {
            try
            {
                if (image == null || image.Empty())
                {
                    LogMessage?.Invoke("❌ 输入图像为空");
                    return null;
                }

                using var croppedImage = new Mat(image, new OpenCvSharp.Rect(region.X, region.Y, region.Width, region.Height));
                
                if (_debugMode)
                {
                    LogMessage?.Invoke($"🔍 在区域({region.X},{region.Y},{region.Width},{region.Height})中识别文字");
                }

                return RecognizeText(croppedImage, preprocessImage);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ 区域OCR识别异常: {ex.Message}");
                return null;
            }
        }

        public int? RecognizeNumber(Mat image, bool preprocessImage = true)
        {
            var result = RecognizeText(image, preprocessImage);
            if (result == null) return null;

            var numbers = Regex.Matches(result.Text, @"\d+");
            if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out var number))
            {
                if (_debugMode)
                {
                    LogMessage?.Invoke($"🔢 识别到数字: {number}");
                }
                return number;
            }

            return null;
        }

        private Mat PreprocessImageForOCR(Mat image)
        {
            try
            {
                Mat grayImage;
                if (image.Channels() > 1)
                {
                    grayImage = new Mat();
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
                }
                else
                {
                    grayImage = image.Clone();
                }

                using var blurred = new Mat();
                Cv2.GaussianBlur(grayImage, blurred, new OpenCvSharp.Size(1, 1), 0);

                var processed = new Mat();
                Cv2.AdaptiveThreshold(blurred, processed, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

                using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(1, 1));
                using var opened = new Mat();
                Cv2.MorphologyEx(processed, opened, MorphTypes.Open, kernel);

                var enlarged = new Mat();
                Cv2.Resize(opened, enlarged, new OpenCvSharp.Size(opened.Width * 2, opened.Height * 2), interpolation: InterpolationFlags.Cubic);

                grayImage.Dispose();

                if (_debugMode)
                {
                    LogMessage?.Invoke("🔄 OCR图像预处理完成：灰度化 → 去噪 → 二值化 → 形态学处理 → 放大");
                }

                return enlarged;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"❌ OCR图像预处理异常: {ex.Message}");
                return image.Clone();
            }
        }

        public bool IsEngineAvailable()
        {
            return _engine != null;
        }

        public List<string> GetSupportedLanguages()
        {
            return GetAvailableLanguages();
        }

        public void ReinitializeEngine()
        {
            lock (_lockObject)
            {
                _engine?.Dispose();
                _engine = null;
                InitializeTesseract();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lockObject)
                {
                    _engine?.Dispose();
                    _engine = null;
                }
                _disposed = true;
                LogMessage?.Invoke("🗑️ OCR服务已释放资源");
            }
        }
    }

    public class OCRResult
    {
        public string Text { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public List<OCRWord> Words { get; set; } = new();
        public DateTime ProcessingTime { get; set; }
        public bool IsSuccess => !string.IsNullOrWhiteSpace(Text) && Confidence > 0.5f;
    }

    public class OCRWord
    {
        public string Text { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public Rectangle BoundingBox { get; set; }
    }
}
