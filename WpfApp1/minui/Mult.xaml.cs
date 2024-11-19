using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static OPL_WpfApp.MainWindow_opl;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using userdata;
using System.ComponentModel.Composition.Primitives;

namespace OPL_WpfApp.minui
{
    /// <summary>
    /// Mult.xaml 的交互逻辑
    /// </summary>
    public partial class Mult : Window
    {
        public Mult()
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
            _ = Getport();


        }

        public async Task Getport()
        {
            Multicast multicast = new Multicast();
            multicast.DataReceived += (sender, message) =>
            {
                // 在主进程中处理接收到的数据
                //[MOTD]§2§l[OPL]§b远程世界 §7-by GLD[/MOTD][AD]25565[/AD]
                Logger.Log($"获取到多播信息: {message}");

                int startIndex = message.IndexOf("[AD]") + 4; // 找到 [AD] 后的位置
                int endIndex = message.IndexOf("[/AD]");       // 找到 [/AD] 的位置

                if (startIndex != -1 && endIndex != -1)
                {
                    string adContent = message.Substring(startIndex, endIndex - startIndex);
                    Logger.Log("AD 标签中的参数是: " + adContent);
                    port_text.Text = adContent;
                }
                else
                {
                    Logger.Log("未找到 AD 标签");
                }

                //string pattern = @"[AD](.*?)[/AD]";
                //Match match = Regex.Match(message, pattern);
                //if (match.Success)
                //{
                //    // 提取匹配到的数字
                //    Logger.Log($"读取到: {match.Groups[3].Value}");
                //    string adValue = match.Groups[1].Value;
                //    port_text.Text = adValue;
                //}
                //else
                //{
                //    Logger.Log("未找到匹配的 AD 标签");
                //}
            };

            // 启动监听
            await multicast.StartListeningAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int port;
            string input = port_text.Text;
            input =input.Replace(" ", "");
            try
            {
                port = int.Parse(input);
                if (!(port > 0 && port <= 65535))
                {
                    MessageBox.Show("端口号不合法，请输入1-65535之间的数字", "错误", MessageBoxButton.OK);
                    return;
                }
            }catch (FormatException)
            {
                MessageBox.Show("端口号不合法，请输入数字", "错误", MessageBoxButton.OK);
                return;
            }
            UserData userData = new UserData();
            string uuid = userData.UUID;
            string output = $"{uuid}:{port}";
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
            MessageBox.Show("已经复制快捷联机码，请粘贴给需要连接的好友", "提示");
            this.Close();
        }
    }
}
