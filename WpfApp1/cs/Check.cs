using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OPL_WpfApp.MainWindow_opl;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using userdata;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ComboBox = System.Windows.Controls.ComboBox;

namespace OPL_WpfApp
{
    public partial class MainWindow_opl : Window
    {
        bool fsterto = false;
        public void Checkopen(string m)
        {
            if (m.Contains("autorunApp start"))
            {
                Logger.Log("[提示]程序启动完毕，请耐心等待隧道连接"); //启动完毕
                fstert.Fill = Brushes.Green;
                fsterto = true;
            }
            if (m.Contains("autorunApp end"))
            {
                Logger.Log("[提示]程序离线，请检查你的网络设置或查看网络连接是否正常");
                fstert.Fill = Brushes.Orange;
                fsterto = false;
            }
            if (m.Contains("LISTEN ON PORT")) //连接成功or断开
            {
                string pattern = @"PORT\s+(\w+:\d+)";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string portInfo = match.Groups[1].Value;
                    if (m.Contains("START"))
                    {
                        Logger.Log("[提示]隧道本地端口为 " + portInfo + " 连接成功");
                        state[portInfo] = 2;
                        Relist();
                        string[] parts = portInfo.Split(':');
                        string type = parts[0];
                        int port = int.Parse(parts[1]);
                        if (type == "tcp")
                        {
                            tcps.Add(new TcpClientWithKeepAlive("127.0.0.1", port));
                            if (!Multicast.IsMulticastOpen() && tcpnum == 1)
                            {
                                Multicast.SetSrcPort(port);
                                _ = Multicast.Seed();
                            }

                        }
                        //else
                        //{
                        //    udps.Add(new UdpClientKeepAlive("127.0.0.1", port));
                        //}
                    }
                    if (m.Contains("END"))
                    {
                        Logger.Log("[错误]隧道本地端口为 " + portInfo + " 断开连接");
                        state[portInfo] = 1;
                        Relist();
                    }

                }
            }
            if (m.Contains("ERROR P2PNetwork login error"))
            {
                Logger.Log("[错误]请检查是否连接网络，或是程序是否拥有网络访问权限！");
                //if (process != null && !process.HasExited)
                state.Clear();
                on = false;
                Relist();
                Strapp();

            }
            if (m.Contains("Only one usage of each socket address"))
            {
               
                Match match = Regex.Match(m, @"listen\s+(tcp|udp)[\s\S]*?(?:0\.0\.0\.0|\\[::\\]|\\[\\S+\\]|localhost|\\S+):(\d+)");
                if (!match.Success) { 
                    string pattern = @"(tcp|udp)\s*:\s*(\d+)";
                    match = Regex.Match(m, pattern);
                }

                if (match.Success && fsterto)
                {
                    // 提取并输出匹配到的协议和端口号
                    string protocol = match.Groups[1].Value;
                    string port = match.Groups[2].Value;
                    Logger.Log($"[错误]: 本地端口{protocol}:{port}被占用，请更换相关本地端口");
                    MessageBox.Show($"本地端口{protocol}:{port}被占用，请更换相关本地端口！！注意！是连接的创建隧道，开房的仅需在无隧道启用情况下启动！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (on) Strapp();
                }
                else if (!fsterto)
                {
                    MessageBox.Show($"注意，你的计算机可能中病毒了！！\r\n请再尝试一次，如果一直这样根据之前的反馈统计情况，如果你每次打开都弹出该窗口，你的计算机极有可能中病毒了，有黑客正在监视你的计算机网络数据，请立即尝试使用杀毒软件\r\n请尝试使用杀毒软件进行全盘查杀，或使用 卡巴斯基病毒清除工具、360系统急救箱或火绒恶性木马专杀工具\r\n等工具进行查杀", "警告", MessageBoxButton.OK, MessageBoxImage.Hand);
                    //MessageBox.Show($"注意，你的计算机可能中病毒了！！\r\n请再尝试一次，如果一直这样根据之前的反馈统计情况，如果你每次打开都弹出该窗口，你的计算机极有可能中病毒了，有黑客正在监视你的计算机网络数据，请立即尝试使用杀毒软件\r\n请尝试使用杀毒软件进行全盘查杀，或使用 卡巴斯基病毒清除工具、360系统急救箱或火绒恶性木马专杀工具\r\n等工具进行查杀", "警告", MessageBoxButton.OK, MessageBoxImage.Hand);
                    if (on) Strapp();
                    sjson.config.Network.TCPPort = sjson.config.Network.TCPPort - 20;
                    sjson.Save();
                }
            }
            if (m.Contains("no such host"))
            {
                Logger.Log("[错误]请检查DNS是否正确，是否连接网络，或是程序是否拥有网络访问权限！");
            }
            if (m.Contains("it will auto reconnect when peer node online"))//对方不在线
            {
                string pattern = @"INFO\s+(\w+)\s+offline";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Logger.Log("[错误]" + id + "不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
                    MessageBox.Show(id + "不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态", "警告");
                }
            }
            if (m.Contains("peer offline"))//对方不在线
            {

                Logger.Log("[错误]你连接的人不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
                //MessageBox.Show("你连接的人不在线！不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态", "警告");

            }
            if (m.Contains("NAT type"))
            {
                string pattern = @"NAT type:(\w+)";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string type = match.Groups[1].Value;
                    //Logger.Log("[提示]你的NAT类型为"+type);
                    if (type == "2")
                        Logger.Log("[提示]你的NAT类型为对称形 Symmetric NAT，连接可能受阻，或连接时间较长");
                }

                string pattern2 = @"publicIP:(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
                Match match2 = Regex.Match(m, pattern2);
                if (match2.Success)
                {
                    string ip = match2.Groups[1].Value;
                    //Logger.Log("[提示]公网IP为" + ip);
                    Net net = new Net();
                    _ = net.Getisp(ip);
                }
            }
            if (m.Contains("login ok")) //登录中心成功
            {
                string pattern = @"node=(\w+)";
                string upattern = @"user=(\w+)";
                Match match = Regex.Match(m, pattern);
                Match umatch = Regex.Match(m, upattern);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Logger.Log("[提示]你的实际UID为" + id);
                }
                //if (umatch.Success)
                //{
                //    string user = umatch.Groups[1].Value;
                //    if (user != "gldoffice")
                //    {

                //        Logger.Log("[错误]：疑似token丢失，开始自动尝试修复");
                //        //MessageBox.Show("疑似token丢失，已自动尝试修复", "严重错误");
                //        Strapp();
                //        sjson.ReSetToken(); //修复token
                //        Logger.Log("[提示]：尝试修复完毕");
                //        Strapp();
                //    }
                //}
            }
        }
        //从https://uapis.cn/api/say获取文本 异步
        public async Task GetsayText(bool oo = true)
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                if (oo) Logger.Log("[提示]获取一言 -UAPI公益API提供数据支持");
                HttpResponseMessage response = await httpClient.GetAsync("https://uapis.cn/api/say");

                // 检查响应状态是否成功
                if (response.IsSuccessStatusCode)
                {
                    // 获取响应内容的字符串形式
                    string contentString = await response.Content.ReadAsStringAsync();
                    daysay.Text = contentString;
                }
            }
            catch (Exception ex)
            {
                daysay.Text = "获取失败";
                Logger.Log("[错误]获取每日一句失败：" + ex.Message);
            }
        }
       
        public static string GetSmBIOSUUID()
        {
            var cmd = "wmic csproduct get UUID";
            return ExecuteCMD(cmd, output =>
            {
                string uuid = GetTextAfterSpecialText(output, "UUID");
                if (uuid == "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                {
                    uuid = null;
                }
                return uuid;
            });
        }
        private static string ExecuteCMD(string cmd, Func<string, string> filterFunc)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.Start();//启动程序
            process.StandardInput.WriteLine(cmd + " &exit");
            process.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return filterFunc(output);
        }
        private static string GetTextAfterSpecialText(string fullText, string specialText)
        {
            if (string.IsNullOrWhiteSpace(fullText) || string.IsNullOrWhiteSpace(specialText))
            {
                return null;
            }
            string lastText = null;
            var idx = fullText.LastIndexOf(specialText);
            if (idx > 0)
            {
                lastText = fullText.Substring(idx + specialText.Length).Trim();
            }
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
                Logger.Log($"[错误]自动复制失败：{ex.Message} - {text}");
                minui.copy_ui ui= new minui.copy_ui(text);
                ui.Owner = App.Current.MainWindow;
                ui.Topmost = true;
                ui.ShowDialog();
                return false;
            }
            return true;
        }
    }
}
