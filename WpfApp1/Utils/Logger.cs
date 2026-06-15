using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace OPL_WpfApp.Utils
{
    /// <summary>
    /// 日志工具类 - 同时输出到 TextBox 和文件
    /// </summary>
    public class Logger
    {
        private static TextBox _output;
        private static string _logFilePath;

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
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string outMessage = string.IsNullOrEmpty(level)
                ? $"[{timestamp}]{message}{Environment.NewLine}"
                : $"[{timestamp}][{level}]{message}{Environment.NewLine}";

            try
            {
                AppendTextToFile(_logFilePath, outMessage);
                if (_output != null)
                    _output.Text = outMessage + _output.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void AppendTextToFile(string absolutePath, string content)
        {
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
