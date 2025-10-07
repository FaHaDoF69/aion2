namespace Aion2Helper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 分配控制台窗口用于调试（开发阶段有用）
            #if DEBUG
            InitializeDebugConsole();
            #endif

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        #if DEBUG
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        /// <summary>
        /// 初始化调试控制台
        /// </summary>
        private static void InitializeDebugConsole()
        {
            try
            {
                AllocConsole();
                SetConsoleTitle("Aion2 调试控制台 - 开发模式");
                
                // 设置控制台颜色
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    Aion2 调试控制台                         ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("║ 启动时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(47) + "║");
                Console.WriteLine("║ 版本信息: v1.0 开发版".PadRight(63) + "║");
                Console.WriteLine("║ 调试模式: 已启用".PadRight(63) + "║");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║ 调试信息类型:".PadRight(63) + "║");
                Console.WriteLine("║  🔍 数据库连接和查询状态".PadRight(63) + "║");
                Console.WriteLine("║  ⚠️  错误和异常详细信息".PadRight(63) + "║");
                Console.WriteLine("║  🖼️  图像识别处理状态".PadRight(63) + "║");
                Console.WriteLine("║  📊 性能监控和统计".PadRight(63) + "║");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                
                Console.WriteLine();
                LogInfo("系统", "调试控制台初始化完成");
                LogInfo("系统", "程序启动中...");
            }
            catch (Exception ex)
            {
                // 如果控制台初始化失败，不影响主程序运行
                System.Diagnostics.Debug.WriteLine($"调试控制台初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 输出信息日志
        /// </summary>
        public static void LogInfo(string category, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{category}] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public static void LogWarning(string category, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{category}] ⚠️  ");
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string category, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{category}] ❌ ");
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// 输出成功日志
        /// </summary>
        public static void LogSuccess(string category, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{category}] ✅ ");
            Console.WriteLine(message);
            Console.ResetColor();
        }
        #endif
    }
}