using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OPL_WpfApp.MainWindow_opl;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp.cs
{
    internal class AddMpPreference
    {
        public void AddMp(string path)
        {
            string filePathToExclude = path; // 指定要排除的文件或文件夹路径

            // 构造PowerShell命令
            string powershellCommand = $"Add-MpPreference -ExclusionPath \"{filePathToExclude}\"";

            // 创建一个新的进程启动信息实例
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{powershellCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // 启动PowerShell进程
            using (Process process = Process.Start(processStartInfo))
            {
                // 读取标准输出和错误输出
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // 等待进程退出
                process.WaitForExit();

                // 打印输出结果
                Logger.Log("Output: " + output);
                Logger.Log("Error: " + error);

                // 检查是否有错误发生
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Logger.Log($"An error occurred while adding exclusion: {error}");
                    MessageBox.Show("自动添加失败，请检查你的杀毒设置，可能因为存在其他杀毒程序，windows安全中心并未工作，请自行添加白名单（使用该功能前请先确认是否可以正常启动，可以正常启动的话无需使用该功能）");
                }
                else
                {
                    Logger.Log("File/Folder has been successfully excluded from Windows Defender.");
                    MessageBox.Show("自动添加成功，请重启程序后再尝试启动(该自动添加排除功能为测试功能)\n如果依然无法启动，请自行查看 Windows安全中心->应用和浏览器控制->智能应用控制设置->关闭 ");
                }
            }
        }
    }
}
