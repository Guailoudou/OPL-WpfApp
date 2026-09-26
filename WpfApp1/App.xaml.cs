using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using OPL_WpfApp.Utils;
using System.Runtime.InteropServices;
using System.Text;


namespace OPL_WpfApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        
        private static Mutex mutex = new Mutex(true, "{OPL_Guailoudou}");
        protected override void OnStartup(StartupEventArgs e)
        {
            string[] args = e.Args;
            if (args.Length == 3 && args[0] == "/service")
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        var currentProcess = Process.GetCurrentProcess();
                        if (int.TryParse(args[2], out int processId))
                        {
                            try
                            {
                                var uiProcess = Process.GetProcessById(processId);
                                // 继续处理
                                if (uiProcess.MainModule == null || currentProcess.MainModule == null)
                                {
                                    // 如果无法获取模块信息，为安全起见，直接返回
                                    return;
                                }
                                if (uiProcess.MainModule.FileName != currentProcess.MainModule.FileName)
                                    return;
                                uiProcess.WaitForExit();
                                Tunnel.Service.Remove(args[1], false);
                            }
                            catch (ArgumentException)
                            {
                                // 进程不存在，直接返回
                                return;
                            }
                        }
                        //var uiProcess = Process.GetProcessById(int.Parse(args[2]));
                        //if (uiProcess.MainModule.FileName != currentProcess.MainModule.FileName)
                        //    return;
                        //uiProcess.WaitForExit();
                        //Tunnel.Service.Remove(args[1], false);
                    }
                    catch { }
                });
                t.Start();
                Tunnel.Service.Run(args[1]);
                t.Interrupt();
                Environment.Exit(0);
                return;
            }
            bool isFirstInstance = true;
            // 检查互斥量是否已被其他实例占用
            try
            {
                isFirstInstance = mutex.WaitOne(0, false);
            }catch (AbandonedMutexException)
            {
                mutex.ReleaseMutex();
            }
            
            if (!isFirstInstance)
            {

                ActivateExistingInstance();
                //mutex.ReleaseMutex();
                //Current.Shutdown();
                Environment.Exit(0); 
                return;
            }
            else
            {
                // 继续
                base.OnStartup(e);
                mutex.ReleaseMutex();
            }
            string OPPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin","openp2p.exe");
            if (!IsProcessElevated())
            {
                // 重启进程并请求管理员权限
                RestartAsAdmin();
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            
            set set = new set();
            if (set.settings?.Color != null && set.settings?.Color != "")
            {
                // 检查 ThemeManager.Current 是否为 null
                if (ThemeManager.Current != null)
                {
                    ThemeManager.Current.AccentColor = set.ParseColor(set.settings.Color);
                    var color = ThemeManager.Current.AccentColor ?? Colors.Black;
                    args = args.Concat(new[] { $"-bg={color}" }).ToArray();
                }
            }

            // 应用程序启动时的自定义逻辑
            var mainWindow = new MainWindow_opl(args);
            mainWindow.Show();
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
            // 应用程序退出时的清理操作
            base.OnExit(e);
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
            string saveOPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.zip");
            if (File.Exists(saveOPath))
            {
                Net coreUpdate = new Net();
                if (coreUpdate.getupdate() && !string.IsNullOrWhiteSpace(coreUpdate.updateInfo.ophash))
                {
                    string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
                    ExtractZipAndOverwrite(saveOPath, binPath, Path.Combine(binPath, "openp2p.exe"), coreUpdate.updateInfo.ophash);
                }
            }
            if (File.Exists(savePath))
            {
                Net net = new Net();
                if (!net.getupdate())
                    return;
                string pathToExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updata.exe");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pathToExe;
                startInfo.Arguments = net.updateInfo.uphash + " " +Process.GetCurrentProcess().MainModule.FileName;
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(pathToExe);
                startInfo.CreateNoWindow = true; // 不显示新的命令行窗口
                try
                {
                    Process.Start(startInfo);
                    Console.WriteLine("更新程序已启动。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"启动程序时发生错误: {ex.Message}");
                }
            }
        }
        public static bool ExtractZipAndOverwrite(string zipPath, string extractPath, string verifyPath = null, string expectedHash = null)
        {
            if (File.Exists(zipPath) && Directory.Exists(extractPath))
            {
                try
                {
                    // 使用ZipFile.OpenRead打开zip文件，这样不会锁定文件
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // 构建解压后文件的完整路径
                            string fullFilePath = Path.Combine(extractPath, entry.FullName);

                            // 确保目录存在
                            if (entry.FullName.EndsWith("/"))
                            {
                                Directory.CreateDirectory(fullFilePath);
                            }
                            else
                            {
                                // 如果文件已存在，则删除旧文件以准备覆盖
                                if (File.Exists(fullFilePath))
                                {
                                    File.Delete(fullFilePath);
                                }

                                // 解压文件到指定路径
                                using (Stream inputStream = entry.Open())
                                using (FileStream outputStream = new FileStream(fullFilePath, FileMode.CreateNew))
                                {
                                    inputStream.CopyTo(outputStream);
                                }
                            }
                        }
                    }

                    if (expectedHash != null && !string.Equals(Net.CalculateMD5Hash(verifyPath), expectedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("解压后的文件 MD5 校验失败。");
                        if (File.Exists(verifyPath)) File.Delete(verifyPath);
                        File.Delete(zipPath);
                        return false;
                    }

                    Console.WriteLine("解压完成，更新完毕。");
                    File.Delete(zipPath);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解压过程中发生错误: {ex.Message}");
                    try { File.Delete(zipPath); } catch { }
                    return false;
                }
            }
            else
            {
                Console.WriteLine("ZIP文件或目标文件夹不存在。");
                return false;
            }
        }
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            var executingAssemblyName = executingAssembly.GetName();
            var resName = executingAssemblyName.Name + ".resources";

            AssemblyName assemblyName = new AssemblyName(args.Name); string path = "";
            if (resName == assemblyName.Name)
            {
                path = executingAssemblyName.Name + ".g.resources"; ;
            }
            else
            {
                path = assemblyName.Name + ".dll";
                if (assemblyName.CultureInfo != null && assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                {
                    path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
                }
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
        private static bool IsProcessElevated()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdmin()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            startInfo.Verb = "runas"; // 请求管理员权限

            try
            {
                Process.Start(startInfo);
                Environment.Exit(0); // 退出当前进程
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Logger.Log("The operation was cancelled by the user.");
            }
            catch (Exception e)
            {
                Logger.Log("Error: " + e.Message);
            }
        }
        // 窗口操作相关
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int SW_RESTORE = 9;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint SC_RESTORE = 0xF120;

        private static void ActivateExistingInstance()
        {
            string windowTitle = "Openp2p Launcher - 联机工具";
            IntPtr hWnd = FindWindow(null, windowTitle);
            if (hWnd != IntPtr.Zero)
            {
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
                    
                }
                SetForegroundWindow(hWnd);
            }
        }

        void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 展开 InnerException 链，避免真实根因被外层异常遮蔽（如 XamlParseException）
            string detail = "";
            Exception ex = e.Exception;
            while (ex != null)
            {
                detail += $"\nMessage: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}";
                ex = ex.InnerException;
                if (ex != null) detail += "\n---- 内部异常 ----";
            }
            MessageBox.Show($"出现未经处理的异常，如果影响到看功能的使用，可以的话，请将该页面截图或日志给开发者，这有助于解决这个问题: {detail}");
            Logger.Log($"Message: {e.Exception.Message}","错误");
            Logger.Log($"Source: {e.Exception.Source}", "错误");
            Logger.Log($"Stack Trace: {e.Exception.StackTrace}", "错误");
            if (e.Exception.InnerException != null)
                Logger.Log($"InnerException: {e.Exception.InnerException}", "错误");
            e.Handled = true;
        }
        

    }
}
