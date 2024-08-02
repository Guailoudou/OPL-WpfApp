using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using userdata;

namespace OPL_WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        UserData userData ;
        json sjson;
        bool on = false;
        int tcpnum = 0;
        public MainWindow(string[] args)
        {
            InitializeComponent();
            // 设置窗口位置居中显示
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
            //userdata.UserData userData = new userdata.UserData();
            //userdata.json sjson = new userdata.json();
            Logger logger = new Logger(richOutput);
            Uplog uplog = new Uplog(uplogbox);
            this.DataContext = userData;
            userData = new userdata.UserData();
            sjson = new userdata.json();
            Logger.Log($"[信息]程序启动，当前版本：{Getversion()}，更新包号：{Net.Getpvn()}");
            
            Net net = new Net();
            _ = net.GetPreset();
            _ = net.Getthank(thank);
            _ = GetsayText();
            Relist();
            UUID.Text = sjson.config.Network.Node;
            share.Text = sjson.config.Network.ShareBandwidth.ToString();
            ver.Content = Getversion();
            //thank.Navigate("https://file.gldhn.top/web/thank/"); 废案，内存占用过高
            if(args.Length > 0 && args[0] == "-on") Strapp();

        }


        private void CopyUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox uuidTextBox = (TextBox)this.FindName("UUID");
            try
            {
                Clipboard.SetText(uuidTextBox.Text);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误]复制失败：{ex.Message}");
                MessageBox.Show($"自动复制可能失败了，尝试手动复制--{ex.Message}", "提示");
                return;
            }
            MessageBox.Show("复制成功", "提示");

        }
        private void CopyipLink(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = (int)button.Tag;
            try
            {
                Clipboard.SetText(iplink[index]);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误]复制失败：{ex.Message}");
                MessageBox.Show($"自动复制可能失败了，尝试手动复制--{ex.Message}", "提示");
                return;
            }
            MessageBox.Show("复制成功", "提示");
        }
        private void ResetUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            // 显示确认对话框
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作!", "警告");
                return;
            }
            MessageBoxResult result = MessageBox.Show(
                "你确定要重置吗?会导致失去所有已有隧道配置!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                userData.ResetUUID();
                TextBox uuidTextBox = (TextBox)this.FindName("UUID");
                uuidTextBox.Text = userData.UUID;
                sjson.newjson(userData);
                MessageBox.Show("已重置UID,新的UID为：" + userData.UUID, "提示");
                Relist();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



        private void Form1_Load(object sender, RoutedEventArgs e)
        {
            // thank.Navigate("https://file.gldhn.top/web/thank/");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作!", "警告");
                return;
            }
            Add Add = new Add();
            Add.Owner = this;
            Add.Topmost = true;
            Add.ShowDialog();
            Relist();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            var CheckBox = (CheckBox)sender;
            int index = (int)CheckBox.Tag;
            
            sjson.onapp(index);
            Relist();
        }
        private void UnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            var CheckBox = (CheckBox)sender;
            int index = (int)CheckBox.Tag;
            sjson.offapp(index);
            Relist();
        }
        private void Del(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            MessageBoxResult result = MessageBox.Show(
                "你确定要删除隧道吗，这是不可逆的!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                var button = (Button)sender;
                int index = (int)button.Tag;
                sjson.del(index);
            }
            else
            {
                return;
            }
            
            Relist();
        }
        private void Edit(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            var button = (Button)sender;
            int index = (int)button.Tag;
            edit ed = new edit(index);
            ed.Owner = this;
            ed.Topmost = true;
            ed.ShowDialog();
            Relist();

        }
        private Dictionary<string, int> state = new Dictionary<string, int>();
       // private Dictionary<string, int> statelist = new Dictionary<string, int>();
        private Dictionary<int, string> iplink = new Dictionary<int, string>();
        public void Relist() //刷新列表
        {
            tcpnum = 0;
            // 获取ListBox控件
            ListBox listBox = this.FindName("sdlist") as ListBox;
            userdata.json json = new userdata.json();
            listBox.Items.Clear();
            iplink.Clear();
            int index = 0;
            if (json.config.Apps != null)
            {
                foreach (userdata.App app in json.config.Apps)
                {
                    if(app.Enabled == 1 ? true : false)
                        if(app.Protocol=="tcp") tcpnum++;

                    // 创建Border
                    Border border = new Border
                    {
                        //BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(5)
                       // Background = new SolidColorBrush(Color.FromArgb(Colors.Black))
                        
                    };
                    SolidColorBrush brush = new SolidColorBrush(); 
                    //brush.Color = Colors.Black;
                    brush.Color = (Color)ColorConverter.ConvertFromString("#0099ff");

                    brush.Opacity = 0.2; 
                    border.Background = brush;

                    // 创建内部Grid
                    Grid grid = new Grid
                    {
                        Height = 63,
                        Width = 700
                    };

                    // 添加各个子控件到Grid
                    grid.Children.Add(new Label
                    {
                        Content = app.AppName + "隧道",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(10, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "目标UID：" + app.PeerNode,
                        Margin = new Thickness(10, 29, 505, 0)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "协议：" + app.Protocol,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(310, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    
                    grid.Children.Add(new Label
                    {
                        Content = "连接： ip:端口 ->",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(310, 32, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    string iplink_str = "127.0.0.1:" + app.SrcPort;
                    iplink[index]= iplink_str;
                    grid.Children.Add(new TextBox
                    {
                        Text = iplink_str,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(415, 36, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFFFFFF")),
                        IsReadOnly = true,
                        ToolTip = new ToolTip
                        {
                            Content = "这是ip和端口，根据具体游戏填写"
                        }
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "远程端口：" + app.DstPort,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(195, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "本地端口：" + app.SrcPort,
                        Margin = new Thickness(195, 32, 386, 0)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "状态：",
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(477, 0, 0, 0)
                    });

                    //if(on&& state[app.Protocol + ":" + app.SrcPort] == 2) ///////////////////////
                    //grid.Children.Add(new Label
                    //{
                    //    Content = "9999ms",
                    //    VerticalAlignment = VerticalAlignment.Top,
                    //    HorizontalAlignment = HorizontalAlignment.Left,
                    //    Margin = new Thickness(532, 0, 0, 0)
                    //});

                    CheckBox checkBox = new CheckBox
                    {
                        Content = "启用",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(630, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        IsChecked = app.Enabled == 1 ? true : false,
                        Tag = index

                    };
                    checkBox.Checked += CheckBox_Checked; // 需要定义CheckBox_Checked事件处理程序
                    checkBox.Unchecked += UnCheckBox_Checked;
                    grid.Children.Add(checkBox);
                    var clo = Brushes.Gray;
                    if (on&&app.Enabled==1)
                    {
                        if (state[app.Protocol + ":" + app.SrcPort] == 1) clo = Brushes.Orange;
                        if(state[app.Protocol + ":" + app.SrcPort] == 2) clo = Brushes.Green;
                    }
                    Ellipse ellipse = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        Fill = clo,
                        Margin = new Thickness(518, 7, 167, 42),
                        ToolTip = new ToolTip 
                        { 
                            Content="灰色：未启动/未启用 橙色：连接中..  绿色：连接成功"
                        }
                    };
                    grid.Children.Add(ellipse);
                    if(!on && !state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        state[app.Protocol + ":" + app.SrcPort] = app.Enabled;
                    if (!on && state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        if(state[app.Protocol + ":" + app.SrcPort]==0)
                            state[app.Protocol + ":" + app.SrcPort] = app.Enabled;


                    Button closeButton = new Button
                    {
                        Content = " X ",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(678, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Tag = index,
                        ToolTip = new ToolTip
                        {
                            Content = "删除隧道，无法恢复"
                        }
                    };
                    closeButton.Click += Del;
                    grid.Children.Add(closeButton);

                    Button editButton = new Button
                    {
                        Content = " 编辑 ",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(575, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = index
                        //Width = 49,
                        //Height = 26
                    };
                    editButton.Click += Edit;
                    grid.Children.Add(editButton);

                    Button copyip = new Button
                    {
                        Content = "复制",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(515, 32, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Tag = index
                    };
                    copyip.Click += CopyipLink;
                    grid.Children.Add(copyip);

                    // 将Grid添加到Border
                    border.Child = grid;
                    index++;
                    // 将Border包装在ListBoxItem中并添加到ListBox
                    ListBoxItem item = new ListBoxItem { Content = border };
                    listBox.Items.Add(item);
                }
            }


        }

        private string Getversion() //获取文件版本号
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.FileVersion;
            return version;
        }
        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Relist();
        }

        private void Button_Click_open(object sender, RoutedEventArgs e)
        {
            Strapp();

        }
        //程序结束后的处理
        private void Stop()
        {
            openbutton.Content = "启动";
            Logger.Log("[提示]----------------------------------程序已停止运行----------------------------------");
            fstert.Fill = Brushes.Gray;
            Multicast.Stop();
            state.Clear();
            on = false;
            Relist();
            _ = Woplog();
        }
        private async Task Woplog()
        {
            string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "openp2p.log");

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
            DateTime Date = DateTime.Now;
            await Task.Delay(500);
            Logger.AppendTextToFile(absolutePath, Environment.NewLine + "[" + Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + "----- OPENP2P Launcher by Guailoudou -----" + Environment.NewLine);

        }
        DateTime OpenDate;
        private void Strapp()
        {
            if (!on&&OpenDate.AddSeconds(1)>DateTime.Now&&OpenDate!=null)
            {
                MessageBox.Show("操作太频繁，请稍后再试 (请至少间隔1s，防止出现BUG)", "警告");
                return;
            }
            OpenDate = DateTime.Now;
            if (process != null && !process.HasExited)
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Kill();
                Stop();
                //if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
                if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
            }
            else
            {
                string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.exe");
                if (!File.Exists(absolutePath))
                {
                    MessageBox.Show("程序文件丢失，无法启动，请从压缩包重新解压bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单", "警告");
                    Logger.Log("[警告]程序文件丢失，无法启动，请从压缩包重新解压bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单");
                    return;

                }
                else Open();

                openbutton.Content = "关闭";
                Logger.Log("[提示]-----------------------程序已开始运行请耐心等待隧道连接----------------------------");
                fstert.Fill = Brushes.Orange;
                on = true;
                Relist();
                //StartMon();
            }
        }
        public static List<UdpClientKeepAlive> udps = new List<UdpClientKeepAlive>();
        public static List<TcpClientWithKeepAlive> tcps = new List<TcpClientWithKeepAlive>();
        private Process process;
        public void Open()
        {

            // 创建进程对象
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "bin/openp2p.exe"; // 控制台应用路径
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true; // 不显示新的命令行窗口

            process = new Process();
            process.StartInfo = startInfo;
            // 设置输出数据接收事件
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Dispatcher.Invoke(() => // 必须在UI线程更新RichTextBox内容
                    {
                        richOutput.AppendText(e.Data + Environment.NewLine);

                        richOutput.ScrollToEnd();

                       
                        Checkopen(e.Data);
                    });
                }
            });

            // 设置错误数据接收事件
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    bool tl = true;
                    Dispatcher.Invoke(() =>
                    {
                        Logger.Log("【错误】: " + e.Data + Environment.NewLine);
                        if (tl)
                        {
                            
                            Strapp();
                            MessageBox.Show("主程序程序openp2p异常退出，请查看软件状态，重新启动","错误");
                            tl = false;
                        }
                    });
                }
            });

            // 启动进程并开始接收输出
            try
            {
                process.Start();

            }
            catch (Exception ex)
            {

                Logger.Log("[错误]启动失败，看来被安全中心拦截" + ex.ToString());
                MessageBox.Show("启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动", "警告");
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
                Stop();
                return;
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        //public void Addbmd()
        //{
        //    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
        //    string cmdpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "addcmd.bat");
        //    Process.Start(cmdpath,path);
        //}
        protected override void OnClosed(EventArgs e)
        {
            
            if (process != null && !process.HasExited)
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Kill();
            }
            Stop();
            if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
            if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
            
            base.OnClosed(e);
        }
        public class Logger
        {
            private static RichTextBox _output;
            private static string absolutePath;
            public Logger(RichTextBox output,bool oon=true)
            {
                _output = output;
                _output.AppendText(Environment.NewLine);
                if(oon)
                    absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "opl.log");
                else
                    absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "openp2p.log");
                Log("----- OPENP2P Launcher by Guailoudou -----");
            }

            public static void Log(string message)
            {
                
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
                DateTime Date = DateTime.Now;
                string outmessage = "[" + Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + message + Environment.NewLine;
                _output.AppendText(outmessage);
                AppendTextToFile(absolutePath, outmessage);

            }
            public static void AppendTextToFile(string absolutePath, string content)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(absolutePath, append: true, encoding: Encoding.UTF8))
                    {
                        writer.Write(content);
                    }
                }
                catch (Exception ex)
                {
                    AppendTextToFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "opl.log"),ex.Message);
                }
            }
        }
        public class Uplog
        {
            private static TextBox _output;

            public Uplog(TextBox output)
            {
                _output = output;
            }

            public static void Log(string message)
            {
                _output.Text=message;
            }
           
        }
        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void share_TextChanged(object sender, TextChangedEventArgs e)
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
            string configfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            try
            {
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    // 添加文件到ZIP存档
                    archive.CreateEntryFromFile(packopllog, System.IO.Path.GetFileName(packopllog));
                    archive.CreateEntryFromFile(packoplog, System.IO.Path.GetFileName(packoplog));
                    archive.CreateEntryFromFile(configfile, System.IO.Path.GetFileName(configfile));
                }
            }catch (Exception e)
            {
                Logger.Log("[错误]日志打包出错，错误信息：" + e.Message);
                MessageBox.Show("日志打包出错，错误信息：" + e.Message, "错误");
            }
                
            Logger.Log("[提示]日志已打包完毕，路径为："+zipFilePath);
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
        DateTime SayTime = DateTime.Now;
        private void Redaysay(object sender, MouseButtonEventArgs e)
        {
            if (SayTime.AddSeconds(3) > DateTime.Now)
            {
                MessageBox.Show("获取频率过快！！间隔需要至少3s", "警告");
                return;
            }
            _ = GetsayText(false);
            SayTime = DateTime.Now;
        }
    }

}
