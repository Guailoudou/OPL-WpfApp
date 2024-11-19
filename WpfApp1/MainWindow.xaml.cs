using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using userdata;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        UserData userData ;
        json sjson;
        bool on = false;
        public static bool over = true;
        int tcpnum = 0;
        public MainWindow_opl(string[] args)
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
            OperatingSystem os = Environment.OSVersion;
            Version vers = os.Version;
            Logger.Log($"[信息]程序启动，当前版本：{Getversion()}，更新包号：{Net.Getpvn()}，系统版本：{vers}");
            if (vers.Major <= 6 && vers.Minor <= 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                Logger.Log("[提示]当前操作系统版本过低，为防止显示问题已自动切换为黑夜模式");

            }else GetTheme();
            
            Net net = new Net();
            _ = net.GetPreset();
            _ = net.Getthank(thank);
            _ = GetsayText();
            Relist();
            UUID.Text = sjson.config.Network.Node;
            share.Text = sjson.config.Network.ShareBandwidth.ToString();
            ver.Content = Getversion() + " - "+ Net.Getpvn();
            string bgColor = ExtractBackgroundColor(args);
            if(bgColor != null) ColorBlock.SelectColor = new SolidColorBrush(set.ParseColor(bgColor));
            //richOutput.SelectionFont = new Font("楷体", 12, FontStyle.Bold);
            //if (args.Contains("-tempon"))
            //{
            //    temp = true;
            //    Strapp();
            //}
            Initialization();
        }
        private static string ExtractBackgroundColor(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-bg=", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = arg.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        return parts[1]; 
                    }
                }
            }
            return null; 
        }

        private void CopyUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(UUID.Text);
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
            MessageBox.Show("复制成功，可在游戏中使用ctrl+v粘贴", "提示");
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
                    object resource = FindResource("list-Background");
                    if (resource is SolidColorBrush bb)
                    {
                        // 从 SolidColorBrush 获取颜色
                        Color color = bb.Color;
                        brush.Color = color;
                    }

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
                        Margin = new Thickness(10, 35, 0, 0)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "协议：" + app.Protocol,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(352, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    
                    grid.Children.Add(new Label
                    {
                        Content = "连接：",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(370, 31, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    string iplink_str = "127.0.0.1:" + app.SrcPort;
                    iplink[index]= iplink_str;
                    grid.Children.Add(new TextBox
                    {
                        Text = iplink_str,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(410, 24, 0, 0),
                        Width=125,
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
                        Margin = new Thickness(220, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "本地端口：" + app.SrcPort,
                        Margin = new Thickness(220, 35, 351, 3)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "状态：",
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(430, 2, 0, 0)
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
                        Margin = new Thickness(540, -3, 0, 34),
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
                        Width = 14,
                        Height = 14,
                        Margin = new Thickness(465, 4, 214, 45),
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

                    string delData = "M5.629,7.5 L6.72612901,18.4738834 C6.83893748,19.6019681 7.77211147,20.4662096 8.89848718,20.4990325 L8.96496269,20.5 L15.0342282,20.5 C16.1681898,20.5 17.1211231,19.6570911 17.2655686,18.5392856 L17.2731282,18.4732196 L18.1924161,9.2527383 L18.369,7.5 L19.877,7.5 L19.6849078,9.40262938 L18.7657282,18.6220326 C18.5772847,20.512127 17.0070268,21.9581787 15.1166184,21.9991088 L15.0342282,22 L8.96496269,22 C7.06591715,22 5.47142703,20.5815579 5.24265599,18.7050136 L5.23357322,18.6231389 L4.121,7.5 L5.629,7.5 Z M10.25,11.75 C10.6642136,11.75 11,12.0857864 11,12.5 L11,18.5 C11,18.9142136 10.6642136,19.25 10.25,19.25 C9.83578644,19.25 9.5,18.9142136 9.5,18.5 L9.5,12.5 C9.5,12.0857864 9.83578644,11.75 10.25,11.75 Z M13.75,11.75 C14.1642136,11.75 14.5,12.0857864 14.5,12.5 L14.5,18.5 C14.5,18.9142136 14.1642136,19.25 13.75,19.25 C13.3357864,19.25 13,18.9142136 13,18.5 L13,12.5 C13,12.0857864 13.3357864,11.75 13.75,11.75 Z M12,1.75 C13.7692836,1.75 15.2083571,3.16379796 15.2491124,4.92328595 L15.25,5 L21,5 C21.4142136,5 21.75,5.33578644 21.75,5.75 C21.75,6.14942022 21.43777,6.47591522 21.0440682,6.49872683 L21,6.5 L14.5,6.5 C14.1005798,6.5 13.7740848,6.18777001 13.7512732,5.7940682 L13.75,5.75 L13.75,5 C13.75,4.03350169 12.9664983,3.25 12,3.25 C11.0536371,3.25 10.2827253,4.00119585 10.2510148,4.93983756 L10.25,5 L10.25,5.75 C10.25,6.14942022 9.93777001,6.47591522 9.5440682,6.49872683 L9.5,6.5 L2.75,6.5 C2.33578644,6.5 2,6.16421356 2,5.75 C2,5.35057978 2.31222999,5.02408478 2.7059318,5.00127317 L2.75,5 L8.75,5 C8.75,3.20507456 10.2050746,1.75 12,1.75 Z";
                    Button closeButton = new Button
                    {

                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(645, 2, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Tag = index,
                        Style = (Style)this.FindResource("svg-button"),
                        ToolTip = new ToolTip
                        {
                            Content = "删除隧道，无法恢复"
                        },
                        Content = new System.Windows.Shapes.Path
                        {
                            Fill = (SolidColorBrush)FindResource("svg-icon"),
                            //Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a4a")),
                            Data = Geometry.Parse(delData)
                        }
                    };
                    closeButton.Click += Del;
                    grid.Children.Add(closeButton);

                    Button editButton = new Button
                    {
                        Content = " 编辑 ",
                        Margin = new Thickness(600,20,0,0),
                        Width = 55,
                        Tag = index
                        //Width = 49,
                        //Height = 26
                    };
                    editButton.Click += Edit;
                    grid.Children.Add(editButton);
                    //string copyData = "M15.0132009,4.5 C16.5733587,4.5 17.1391096,4.66244482 17.70948,4.96748223 C18.2798504,5.27251964 18.7274804,5.72014965 19.0325178,6.29052002 L19.1342249,6.49326214 C19.3735291,7.00777167 19.5,7.61018928 19.5,8.9867991 L19.5,17.5132009 C19.5,19.0733587 19.3375552,19.6391096 19.0325178,20.20948 C18.7274804,20.7798504 18.2798504,21.2274804 17.70948,21.5325178 L17.5067379,21.6342249 C16.9922283,21.8735291 16.3898107,22 15.0132009,22 L6.4867991,22 C4.92664131,22 4.36089039,21.8375552 3.79052002,21.5325178 C3.22014965,21.2274804 2.77251964,20.7798504 2.46748223,20.20948 L2.36577509,20.0067379 C2.12647088,19.4922283 2,18.8898107 2,17.5132009 L2,8.9867991 C2,7.42664131 2.16244482,6.86089039 2.46748223,6.29052002 C2.77251964,5.72014965 3.22014965,5.27251964 3.79052002,4.96748223 L3.99326214,4.86577509 C4.47347103,4.64242449 5.03025764,4.51736427 6.22159636,4.50168224 L15.0132009,4.5 Z M6.4867991,6 C5.29081707,6 4.8991107,6.07564199 4.49791831,6.29020203 C4.18895065,6.45543974 3.95543974,6.68895065 3.79020203,6.99791831 L3.71163699,7.15981826 C3.56872488,7.49032199 3.50932077,7.88419566 3.50102731,8.75808525 L3.5,17.5132009 C3.5,18.7091829 3.57564199,19.1008893 3.79020203,19.5020817 C3.95543974,19.8110494 4.18895065,20.0445603 4.49791831,20.209798 L4.65981826,20.288363 C4.99032199,20.4312751 5.38419566,20.4906792 6.25808525,20.4989727 L6.4867991,20.5 L15.0132009,20.5 L15.4506279,20.4958158 C16.3138066,20.4773591 16.6543816,20.39575 17.0020817,20.209798 C17.3110494,20.0445603 17.5445603,19.8110494 17.709798,19.5020817 L17.788363,19.3401817 C17.9312751,19.009678 17.9906792,18.6158043 17.9989727,17.7419147 L18,17.5132009 L18,8.9867991 L17.9958158,8.54937207 C17.9773591,7.6861934 17.89575,7.34561838 17.709798,6.99791831 C17.5445603,6.68895065 17.3110494,6.45543974 17.0020817,6.29020203 L16.8401817,6.21163699 C16.509678,6.06872488 16.1158043,6.00932077 15.2419147,6.00102731 L6.4867991,6 Z M15.590287,2 C17.8190838,2 18.6272994,2.23206403 19.4421143,2.66783176 C20.2569291,3.10359949 20.8964005,3.74307093 21.3321682,4.55788574 C21.767936,5.37270056 22,6.18091615 22,8.409713 L22,14.3722296 C22,16.1552671 21.8143488,16.8018396 21.4657346,17.4536914 C21.2202776,17.9126561 20.8940321,18.3020799 20.4949174,18.6140435 C20.4981214,18.4695118 20.5,18.316606 20.5,18.1541722 L20.5,8.3458278 C20.5,6.97563815 20.3663256,6.28341544 19.9811141,5.56313259 C19.6264537,4.89997522 19.1000248,4.3735463 18.4368674,4.01888586 C17.7646034,3.65935514 17.1167829,3.51894119 15.9193798,3.50181725 L5.8458278,3.5 C5.68310622,3.5 5.5299464,3.50188529 5.38516578,3.50585327 C5.69792007,3.10596794 6.0873439,2.77972241 6.54630859,2.53426541 C7.19816044,2.18565122 7.84473292,2 9.6277704,2 L15.590287,2 Z M10.75,8.25 C11.1642136,8.25 11.5,8.58578644 11.5,9 L11.5,17.5 C11.5,17.9142136 11.1642136,18.25 10.75,18.25 C10.3357864,18.25 10,17.9142136 10,17.5 L10,14 L6.5,14 C6.08578644,14 5.75,13.6642136 5.75,13.25 C5.75,12.8357864 6.08578644,12.5 6.5,12.5 L10,12.5 L10,9 C10,8.58578644 10.3357864,8.25 10.75,8.25 Z M15,12.5 C15.4142136,12.5 15.75,12.8357864 15.75,13.25 C15.75,13.6642136 15.4142136,14 15,14 L12.5,14 L12.5,12.5 L15,12.5 Z";
                    Button copyip = new Button
                    {
                        Margin = new Thickness(540, 20, 0, 0),
                        Width=55,
                        Tag = index,
                        Content="复制"
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
            if(over)
                Strapp();
            else
            {
                MessageBox.Show("暂时无法启动，正在进行关键文件下载，请等待片刻，如果下载失败可以尝试下载压缩包，并解压使用，完成下载后会有弹窗提示", "提示");
            }

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
            string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"bin", "log", "openp2p.log");

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
            DateTime Date = DateTime.Now;
            await Task.Delay(500);
            Logger.AppendTextToFile(absolutePath, Environment.NewLine + "[" + Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + "----- OPENP2P Launcher by Guailoudou -----" + Environment.NewLine);

        }
        DateTime OpenDate;
        private void Strapp()
        {
            //if (!IsProcessElevated())
            //{
            //    // 重启进程并请求管理员权限
            //    RestartAsAdmin();
            //    return;
            //}
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
                Logger.Log("-----------------------程序已开始运行请耐心等待隧道连接----------------------------","提示");
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
            startInfo.FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin","openp2p.exe"); // 控制台应用路径
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
                        //richOutput.AppendText(e.Data + Environment.NewLine);
                        richOutput.Text = e.Data + Environment.NewLine + richOutput.Text;
                        //richOutput.ScrollToEnd(); 



                        Checkopen(e.Data);
                    });
                }
            });
            bool tl = true;
            // 设置错误数据接收事件
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    
                    Dispatcher.Invoke(() =>
                    {
                        Logger.Log("【错误】: " + e.Data + Environment.NewLine);
                        if (tl)
                        {

                            if (process != null && !process.HasExited)
                            {
                                process.CancelOutputRead();
                                process.CancelErrorRead();
                                process.Kill();
                                
                                //if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
                                if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
                            }
                            Stop();
                            Logger.Log("主程序程序openp2p崩掉了！请查看软件状态，尝试重新启动，或联系作者","错误");
                            tl = false;
                            _ =  DelayAndExecute();
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
                MessageBox.Show("启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动\r内网穿透程序常被黑客用来用来入侵企业内网，故非常容易报毒，请信任程序的安全性\r请进行如下操作：Windows安全中心->病毒和威胁防护->“病毒和威胁防护”设置->管理设置->排除项->添加或删除排除项->添加排除项->文件夹  添加以下路径\r"+ System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin") + "\r"+AppDomain.CurrentDomain.BaseDirectory, "警告");
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
        private async Task timeopen()
        {
            await Task.Delay(3000);
            Strapp();
        }
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
            private static TextBox _output;
            private static string absolutePath;
            public Logger(TextBox output,bool oon=true)
            {
                _output = output;
                _output.FontFamily = new FontFamily("Times New Roman"); 
                if(oon)
                    absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin","log", "opl.log");
                else
                    absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "openp2p.log");
                Log("----- OPENP2P Launcher by Guailoudou -----");
            }

            public static void Log(string message)
            {
                
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
                DateTime Date = DateTime.Now;
                string outmessage = "[" + Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]" + message + Environment.NewLine;
                //_output.AppendText(outmessage);
                _output.Text = outmessage + _output.Text ;
                //_output.ScrollToEnd();
                AppendTextToFile(absolutePath, outmessage);

            }
            public static void Log(string message,string log)
            {

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
                DateTime Date = DateTime.Now;
                string outmessage = "[" + Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][" + log + "]" + message + Environment.NewLine;
                //_output.AppendText(outmessage);
                _output.Text = outmessage + _output.Text;
                //_output.ScrollToEnd();
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
                    AppendTextToFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log"),ex.Message);
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
            string newpackopllog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "opl.log");
            string newpackoplog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "log", "openp2p.log");
            string configfile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            try
            {
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    // 添加文件到ZIP存档
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

        private void Quick_Add(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作! 操作无效", "警告");
                Relist();
                return;
            }
            string pastedText = Clipboard.GetText();
            pastedText = pastedText.Replace("\r", "");
            pastedText = pastedText.Replace("\n", "");
            pastedText = pastedText.Replace(" ", "");
            pastedText = pastedText.Replace("：", ":");
            pastedText = pastedText.Replace("；", ";");
            try
            {
                if(pastedText=="") throw new ArgumentException("无效码");
                var connections = ConnectionParser.ParseConnections(pastedText);
                json json = new json();
                json.Alloff();
                foreach (var conn in connections)
                {
                    string type = conn.Protocol;
                    if (type == "1") type = "tcp";
                    if (type == "2") type = "udp";
                    string uid = conn.UID;
                    int port = conn.Port;
                    int cport = conn.CPort;
                    json.Add1link(type,uid,port,cport);
                }
                Relist();
                MessageBox.Show("已将列表状态同步连接码", "提示");
            }
            catch (Exception ex)
            {
                Logger.Log($"无法识别的连接码: {pastedText} - {ex.Message}");
                MessageBox.Show($"无法识别的连接码: {pastedText} \n请复制连接码后点击\r该功能为一键添加/编辑隧道为连接码隧道，房主可直接编辑发送连接码供连接方使用。 \r\r连接码用法： \r用法1：\r uid:端口 --> tcp协议连接码 \r示例：qwertyuioop:25565 \r\r 用法2：\r<1/2>:uid:端口[:本地端口] --> 1为tcp，2为udp 本地端口可省略\r示例：1:qwertyuiop:25565:25575 \r多个连接可以用;间隔同时输入\r复制后直接点击该按钮即可完成添加，后直接启动即可  \r如果确认你复制的符合格式，可尝试点击右边按钮自行添加隧道\r\r {ex.Message}", "错误");
            }
        }

        private void copysss_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(daysay.Text);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误]复制失败：{ex.Message}");
                MessageBox.Show($"自动复制可能失败了，--{ex.Message}", "提示");
                return;
            }
            MessageBox.Show("复制成功", "提示");
        }

        
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OperatingSystem os = Environment.OSVersion;
            Version vers = os.Version;
            
            if (vers.Major <= 6 && vers.Minor <= 1)
            {
                MessageBox.Show("当前系统过低，主题设置可能存在意想不到的后果", "警告");
                //return;
            }
             var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "跟随系统":
                        ThemeManager.Current.ApplicationTheme = null;
                        SetThene("");
                        break;
                    case "浅色":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        SetThene("Light");
                        break;
                    case "深色":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        SetThene("Dark");
                        break;
                }
            }
        }
        
        private void ColorPicker_Set(object sender, RoutedEventArgs e)
        {
            
            SolidColorBrush color = ColorBlock.SelectColor;
            ThemeManager.Current.AccentColor = color.Color;
            set set = new set();
            set.settings.Color = color.Color.ToString();
            set.Write();
            MessageBox.Show($"设置颜色成功{color.Color} 部分样式可能需要重启生效");
        }

        private void ColorPicker_ReSet(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Color = "";
            set.Write();
            MessageBox.Show($"已重置，重启生效");
        }

        private void SetReColor(object sender, RoutedEventArgs e)
        {
            var Border = sender as Border;
            SolidColorBrush color = Border.Background as SolidColorBrush;
            ThemeManager.Current.AccentColor = color.Color;
            ColorBlock.SelectColor = color;
            set set = new set();
            set.settings.Color = color.Color.ToString();
            set.Write();
            MessageBox.Show($"设置颜色成功{color.Color} 部分样式可能需要重启生效");
        }

        private void Autoup_op(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_upop = true;
            set.Write();
        }

        private void UnAutoup_op(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "你确定要关闭openp2p文件校验吗，你需要知道你在做什么，如果你不了解这个的作用请不用动他！！!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question); 
            if (result == MessageBoxResult.OK)
            {
                set set = new set();
                set.settings.Auto_upop = false;
                set.Write();
            }
            else
            {
                Autoup_opn.IsChecked = true;
                return;
            }
            
        }

        private void Autoup(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_up = true;
            set.Write();
        }

        private void UnAutoup(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "你确定要关闭自动升级吗，你需要知道你在做什么，这可能会导致你的程序存在bug！！!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                set set = new set();
                set.settings.Auto_up = false;
                set.Write();
            }
            else
            {
                Autoupn.IsChecked = true;
                return;
            }
            
        }
        async Task DelayAndExecute()
        {
            Logger.Log("将在2s后自动重新启动...");
            await Task.Delay(2000);
            Strapp();
        }
        private void Initialization(bool temp =false)
        {
            set set = new set();
            if (set.settings.Auto_upop)
            {
                Autoup_opn.IsChecked = true;
            }
            if(set.settings.Auto_up)
            {
                Autoupn.IsChecked = true;
            }
            if (set.settings.Auto_open)
            {
                Autoup_openn.IsChecked = true;
                if(!temp)Strapp();
            }
            if (AutoStartWith.IsInStartup())
            {
                Autoup_bootn.IsChecked = true;
            }
            if (set.settings.ispwarning)
            {
                Ispwarning.IsChecked = true;
            }
            Autoup_bootn.Checked += Auto_boot;
            Autoup_bootn.Unchecked += UnAuto_boot;

            Autoup_opn.Checked += Autoup_op;
            Autoup_opn.Unchecked += UnAutoup_op;

            Autoupn.Checked += Autoup;
            Autoupn.Unchecked += UnAutoup;

            Autoup_openn.Checked += Auto_open;
            Autoup_openn.Unchecked += UnAuto_open;

            Ispwarning.Checked += Ispwarn;
            Ispwarning.Unchecked += UnIspwarn;

            string newuuid;
            try
            {
                newuuid = GetSmBIOSUUID();
                if (newuuid == null) throw new ArgumentException("未获取到正确数据");
            }
            catch (Exception e)
            {
                Logger.Log("获取设备id失败 : "+e.Message,"错误");
                return;
            }
            Logger.Log("设备ID为："+newuuid,"信息");
            if(set.settings.csproduct==null||set.settings.csproduct=="")
                set.settings.csproduct = newuuid;
            else if(set.settings.csproduct!=newuuid)
            {
                MessageBoxResult result = MessageBox.Show(
                "检测到设备更改，是否要重置UID？（如果这是别人发你的，请点确认，否则可能会导致运行问题）",
                "提示",
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
                set.settings.csproduct = newuuid;
            }
            set.Write();

        }

        private void Auto_boot(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("开启此功能后开机仍然可能会弹出以管理员启动的同意弹窗\r想要不提示，可以在电脑底部搜索框输入UAC，打开”更改用户账户控制设置“把权限降到最低即可", "提示");
            AutoStartWith.AddToStartup();
        }

        private void Auto_open(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_open = true;
            set.Write();
        }

        private void UnAuto_boot(object sender, RoutedEventArgs e)
        {
            AutoStartWith.RemoveFromStartup();

        }

        private void UnAuto_open(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_open = false;
            set.Write();
        }

        private void Button_help(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://blog.gldhn.top/2024/07/12/oplwin_help/");
            }
            catch { }
        }

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            json json = new json();
            json.Alloff();
            Relist();
        }

        private void Ispwarn(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.ispwarning = true;
            set.Write();
        }

        private void UnIspwarn(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.ispwarning = false;
            set.Write();
        }

        private void Outlist(object sender, RoutedEventArgs e)
        {
            string output = "";
            json json = new json();
            if (json.config.Apps != null)
            {
                foreach (userdata.App app in json.config.Apps)
                {
                    if (app.Enabled == 1)
                    {
                        if(output!="")output += ";";
                        output += app.Protocol == "tcp" ? 1 : 2 ;
                        output +=  ":" + app.PeerNode + ":" + app.DstPort + ":" + app.SrcPort;
                    }
                }
                if (output == "")
                {
                    MessageBox.Show("你目前没有启用的隧道，无法导出，请将需要导出的隧道启用", "提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("你目前没有隧道，无法导出", "提示");
                return;
            }
            try
            {
                Clipboard.SetText(output);
            }
            catch (Exception ex)
            {
                Logger.Log($"[错误]复制失败：{ex.Message} - {output}");
                MessageBox.Show($"自动复制可能失败了，--{ex.Message}", "提示");
                return;
            }
            MessageBox.Show("已经将启用的隧道导出为连接码，并已复制，可粘贴保存，复制连接码点击添加左边加号可添加", "提示");
        }

        private void Multput(object sender, RoutedEventArgs e)
        {
            minui.Mult mult = new minui.Mult();
            mult.Owner = this;
            mult.Topmost = true;
            mult.ShowDialog();
            Relist();
        }
    }

}
