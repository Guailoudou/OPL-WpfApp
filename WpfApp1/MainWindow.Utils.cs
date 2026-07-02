using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using OPL_WpfApp.Utils;
using OPL_WpfApp.cs;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// 工具方法
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        private string Getversion() // 获取文件版本号
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.FileVersion;
            return version;
        }

        private void share_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string sshare = share.Text;
            int shares;
            try
            {
                shares = int.Parse(sshare.Replace(" ", ""));
            }
            catch {
                MessageBox.Show("错误的输入", "错误");
                share.Text = sjson.config.Network.ShareBandwidth.ToString();
                return;
            }
            sjson.Setshare(shares);
        }

        public void DerLog(bool oon =true)
        {
            DateTime Date = DateTime.Now;
            string zipFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log-pack-"+Date.ToString("yyyyMMdd-HHmmssfff") +".zip");
            string packoplog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin","bin","log","openp2p.log");
            string packopllog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "opl.log");
            string newpackopllog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log");
            string newpackoplog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "openp2p.log");
            string configfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            try
            {
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    if (File.Exists(packopllog))
                        archive.CreateEntryFromFile(packopllog, System.IO.Path.Combine("old",System.IO.Path.GetFileName(packopllog)));
                    if (File.Exists(packoplog))
                        archive.CreateEntryFromFile(packoplog, System.IO.Path.Combine("old", System.IO.Path.GetFileName(packoplog)));
                    if (File.Exists(configfile))
                        archive.CreateEntryFromFile(configfile, System.IO.Path.GetFileName(configfile));
                    if (File.Exists(newpackopllog))
                        archive.CreateEntryFromFile(newpackopllog, System.IO.Path.GetFileName(newpackopllog));
                    if (File.Exists(newpackoplog))
                        archive.CreateEntryFromFile(newpackoplog, System.IO.Path.GetFileName(newpackoplog));
                }
            }catch (Exception e)
            {
                Logger.Log("[错误] 日志打包出错，错误信息：" + e.Message);
                MessageBox.Show("日志打包出错，错误信息：" + e.Message, "错误");
            }
                
            Logger.Log("[提示] 日志已打包完毕，路径为："+zipFilePath);
            if (oon)
            {
                MessageBox.Show("日志已打包完毕，路径为：" + zipFilePath + "\n即将自动打开根目录文件夹", "提示");
                try
                {
                    Process.Start("explorer.exe", zipFilePath + ",/select");
                }
                catch { }
            }
        }

        private void ExportLog(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作!", "警告");
                return;
            }
            DerLog();
        }

        private void Openwiki(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://blog.gldhn.top/2024/04/19/opl_ui/");
            }
            catch { }
        }

        private void OpenMe(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://space.bilibili.com/496960407");
            }
            catch { }
        }

        private void OpenGit(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/Guailoudou/OPL-WpfApp");
            }
            catch { }
        }

        private void Button_help(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://blog.gldhn.top/2024/07/12/oplwin_help/");
            }
            catch { }
        }

        private void Multput(object sender, RoutedEventArgs e)
        {
            minui.Mult mult = new minui.Mult();
            mult.Owner = this;
            mult.Topmost = true;
            mult.ShowDialog();
        }

        private void AddMpBotton(object sender, RoutedEventArgs e)
        {
            string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            AddMpPreference addMpPreference = new AddMpPreference();
            addMpPreference.AddMp(absolutePath);
        }

        /// <summary>
        /// 获取公告并检查是否有新公告，有则弹窗展示
        /// </summary>
        private async Task CheckAndShowNewNotices()
        {
            var notices = await net.Getnotice(notice);
            if (notices == null || notices.Count == 0)
                return;

            string lastTime = set.settings.LastNoticeTime;

            var newNotices = new System.Collections.Generic.List<notice>();
            foreach (var item in notices)
            {
                if (string.IsNullOrEmpty(lastTime) || string.Compare(item.time, lastTime, StringComparison.Ordinal) > 0)
                {
                    newNotices.Add(item);
                }
            }

            if (newNotices.Count == 0)
                return;

            string msgTitle = newNotices.Count == 1
                ? "📢 有新的公告"
                : $"📢 有 {newNotices.Count} 条新公告";
            string msgContent = "";
            foreach (var item in newNotices)
            {
                msgContent += $"【{item.title}】({item.time})\n{item.content}\n\n";
            }

            MessageBox.Show(msgContent.TrimEnd(), msgTitle);

            set.settings.LastNoticeTime = newNotices[newNotices.Count - 1].time;
            set.Write();
        }

        public static string GetSmBIOSUUID()
        {
            var cmd = "wmic csproduct get UUID";
            return ExecuteCMD(cmd, output =>
            {
                string uuid = GetTextAfterSpecialText(output, "UUID");
                if (uuid == "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                    uuid = null;
                return uuid;
            });
        }

        private static string ExecuteCMD(string cmd, Func<string, string> filterFunc)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.WriteLine(cmd + " &exit");
            process.StandardInput.AutoFlush = true;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return filterFunc(output);
        }

        private static string GetTextAfterSpecialText(string fullText, string specialText)
        {
            if (string.IsNullOrWhiteSpace(fullText) || string.IsNullOrWhiteSpace(specialText))
                return null;

            string lastText = null;
            var idx = fullText.LastIndexOf(specialText);
            if (idx > 0)
                lastText = fullText.Substring(idx + specialText.Length).Trim();

            return lastText;
        }

        public static bool Copy_text(string text)
        {
            try
            {
                System.Windows.Clipboard.SetDataObject(text);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误] 自动复制失败：{ex.Message} - {text}");
                minui.copy_ui ui = new minui.copy_ui(text);
                ui.Owner = App.Current.MainWindow;
                ui.Topmost = true;
                ui.ShowDialog();
                return false;
            }
            return true;
        }

        private void RichTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }

        private void Form1_Load(object sender, RoutedEventArgs e)
        {
        }
    }
}
