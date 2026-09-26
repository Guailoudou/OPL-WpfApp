using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace OPL_WpfApp.Utils
{
    /// <summary>
    /// 日志工具类 - 同时输出到 TextBox 和文件。
    /// 性能优化：界面日志行数封顶（避免无限增长吃内存），
    /// 窗口隐藏时只缓冲不刷新界面，回到前台后一次性补刷。
    /// </summary>
    public class Logger
    {
        private static TextBox _output;
        // 静态默认路径：保证实例构造前（启动极早期）调用 Log 也不会因 path 为 null 而崩掉真实错误
        private static readonly string DefaultOplLogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log");
        private static string _logFilePath = DefaultOplLogPath;
        private static readonly Queue<string> _pending = new Queue<string>();
        private const int PendingCap = 500; // 后台缓冲上限，防止长时间隐藏无界增长

        /// <summary>界面日志最大保留行数（可由设置调整）</summary>
        public static int MaxLines { get; set; } = 300;

        /// <summary>是否写入日志文件</summary>
        public static bool FileLogEnabled { get; set; } = true;

        /// <summary>是否在日志中显示时间戳</summary>
        public static bool TimestampEnabled { get; set; } = true;

        /// <summary>界面是否可见（由 PerformanceManager 维护）</summary>
        public static bool UiVisible { get; set; } = true;

        public Logger(TextBox output, bool useOplLog = true)
        {
            _output = output;
            if (_output != null)
                _output.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");

            _logFilePath = useOplLog
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "openp2p.log");

            Log("----- OPENP2P Launcher by Guailoudou -----");
        }

        public static void Log(string message)
        {
            Log(message, null);
        }

        public static void Log(string message, string level)
        {
            string timestamp = TimestampEnabled ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") : null;
            string outMessage = TimestampEnabled
                ? (string.IsNullOrEmpty(level)
                    ? $"[{timestamp}]{message}{Environment.NewLine}"
                    : $"[{timestamp}][{level}]{message}{Environment.NewLine}")
                : (string.IsNullOrEmpty(level) ? message + Environment.NewLine : $"[{level}]{message}{Environment.NewLine}");

            try
            {
                if (FileLogEnabled)
                    AppendTextToFile(_logFilePath, outMessage);
                if (_output != null)
                {
                    if (UiVisible)
                    {
                        AppendToUi(outMessage);
                    }
                    else
                    {
                        lock (_pending)
                        {
                            if (_pending.Count >= PendingCap) _pending.Dequeue();
                            _pending.Enqueue(outMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void AppendToUi(string outMessage)
        {
            string text = outMessage + _output.Text;
            // 封顶裁剪：超过 MaxLines 行则丢弃最旧内容，避免日志无限增长占用内存
            int limit = MaxLines < 20 ? 20 : MaxLines;
            int idx = 0;
            for (int n = 0; n < limit; n++)
            {
                idx = text.IndexOf('\n', idx);
                if (idx < 0) { idx = text.Length; break; }
                idx++;
            }
            if (idx < text.Length)
                text = text.Substring(0, idx);
            _output.Text = text;
        }

        /// <summary>把后台期间缓冲的日志一次性补刷到界面（按新→旧顺序）</summary>
        public static void FlushPending()
        {
            if (_output == null) return;
            string[] batch;
            lock (_pending)
            {
                if (_pending.Count == 0) return;
                batch = _pending.ToArray();
                _pending.Clear();
            }
            var sb = new StringBuilder();
            for (int i = batch.Length - 1; i >= 0; i--) sb.Append(batch[i]);
            AppendToUi(sb.ToString());
        }

        public static void AppendTextToFile(string absolutePath, string content)
        {
            if (string.IsNullOrEmpty(absolutePath))
                absolutePath = DefaultOplLogPath;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                using (StreamWriter writer = new StreamWriter(absolutePath, append: true, encoding: Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
            catch (Exception ex)
            {
                // 回退写入 opl.log
                string fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log");
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fallbackPath));
                    using (StreamWriter writer = new StreamWriter(fallbackPath, append: true, encoding: Encoding.UTF8))
                    {
                        writer.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}][Logger错误]{ex.Message}{Environment.NewLine}");
                    }
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// 更新日志显示工具 - 将文本显示在指定 TextBox 中
    /// </summary>
    public class Uplog
    {
        private static TextBox _output;

        public Uplog(TextBox output)
        {
            _output = output;
        }

        public static void Log(string message)
        {
            if (_output != null)
                _output.Text = message;
        }
    }
}
