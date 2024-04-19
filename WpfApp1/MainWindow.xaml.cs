using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using userdata;

namespace OPL_WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        userdata.UserData userData = new userdata.UserData();
        userdata.json sjson = new userdata.json();
        bool on = false;
        public MainWindow()
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
            this.DataContext = userData;
            Relist();
            Logger logger = new Logger(richOutput);
            Uplog uplog = new Uplog(uplogbox);
            Net net = new Net();
            _ = net.GetPreset();

            
            //sjson.newjson(userData);
            //wpfWebBrowser.Navigate("https://blog.gldhn.top/2024/04/15/opl_help/");
        }
        //userdata.UserData userData = new userdata.UserData();
        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //MessageBox.Show("你左键点击了我", "提示");
        //}

        private void CopyUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox uuidTextBox = (TextBox)this.FindName("UUID");
            Clipboard.SetText(uuidTextBox.Text);
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
                MessageBox.Show("已重置UUID,新的UUID为：" + userData.UUID, "提示");
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
            // wpfWebBrowser.Navigate("https://blog.gldhn.top/2024/04/15/opl_help/");
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
        }
        private void Del(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            var button = (Button)sender;
            int index = (int)button.Tag;
            sjson.del(index);
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
        public void Relist()
        {

            // 获取ListBox控件
            ListBox listBox = this.FindName("sdlist") as ListBox;
            userdata.json json = new userdata.json();
            listBox.Items.Clear();
            int index = 0;
            if (json.config.Apps != null)
            {
                foreach (userdata.App app in json.config.Apps)
                {
                    // 创建Border
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };

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
                        Content = "目标UUID：" + app.PeerNode,
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

                    grid.Children.Add(new TextBox
                    {
                        Text = "127.0.0.1:" + app.SrcPort,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(415, 36, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        IsReadOnly = true
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

                    Button closeButton = new Button
                    {
                        Content = " X ",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(678, 4, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Tag = index
                    };
                    closeButton.Click += Del;
                    grid.Children.Add(closeButton);

                    Button editButton = new Button
                    {
                        Content = "编辑",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(576, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = index
                    };
                    editButton.Click += Edit;
                    grid.Children.Add(editButton);

                    // 将Grid添加到Border
                    border.Child = grid;
                    index++;
                    // 将Border包装在ListBoxItem中并添加到ListBox
                    ListBoxItem item = new ListBoxItem { Content = border };
                    listBox.Items.Add(item);
                }
            }


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Relist();
        }

        private void Button_Click_open(object sender, RoutedEventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Kill();
                openbutton.Content = "启动";
                Logger.Log("[提示]----------------------------------程序已停止运行----------------------------------");
                on = false;
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
                on = true;
                //StartMon();
            }

        }
        //public async void StartMon()
        //{
        //    await Task.Run(async () => await Oph());
        //}
        //private async Task Oph()
        //{
        //    Logger.Log("[提示]开始监控主程序");
        //    while(on) {
        //        await Task.Delay(5000);
        //        if (process != null && !process.HasExited)
        //        {
        //            openbutton.Content = "启动";
        //            Logger.Log("[警告]-----------------------程序已异常停止运行----------------------------");
        //            MessageBox.Show("主程序异常停止运行，请尝试新开启", "警告");
        //            on = false;

        //        }
        //    }
        //    Logger.Log("[提示]退出主程序监控");

        //}
        private Process process;
        public void Open()
        {
            // 创建进程对象
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "bin/openp2p.exe"; // 替换为你的控制台应用路径
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true; // 不显示新的命令行窗口

            process = new Process();
            process.StartInfo = startInfo;
            userdata.outcheck check = new userdata.outcheck();
            // 设置输出数据接收事件
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Dispatcher.Invoke(() => // 必须在UI线程更新RichTextBox内容
                    {
                        richOutput.AppendText(e.Data + Environment.NewLine);

                        richOutput.ScrollToEnd();

                        check.Check(e.Data);
                    });
                }
            });

            // 设置错误数据接收事件
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Dispatcher.Invoke(() =>
                    {
                        richOutput.AppendText("【错误】: " + e.Data + Environment.NewLine);
                    });
                }
            });

            // 启动进程并开始接收输出
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

        }
        protected override void OnClosed(EventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Kill();
            }
            base.OnClosed(e);
        }
        public class Logger
        {
            private static RichTextBox _output;

            public Logger(RichTextBox output)
            {
                _output = output;
            }

            public static void Log(string message)
            {
                string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "bin", "log", "opl.log");
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
                    MessageBox.Show(ex.Message, "错误");
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
    }

}
