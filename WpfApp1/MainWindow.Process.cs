using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using iNKORE.UI.WPF.Modern.Controls;
using OPL_WpfApp.Utils;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// OpenP2P 进程管理
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        public static List<UdpClientKeepAlive> udps = new List<UdpClientKeepAlive>();
        public static List<TcpClientWithKeepAlive> tcps = new List<TcpClientWithKeepAlive>();
        private Process process;
        DateTime OpenDate;
        bool fsterto = false;
        int fsterton = 0;

        private void Button_Click_open(object sender, RoutedEventArgs e)
        {
            if(over)
                Strapp();
            else
            {
                MessageBox.Show("暂时无法启动，正在进行关键文件下载，请稍候再试，如果下载失败可以尝试下载压缩包，并解压使用，完成下载后会有日志提示", "提示");
            }
        }

        private void Strapp(bool isSys=false)
        {
            if (eton)
            {
                MessageBox.Show("组网运行情况下无法运行该模块，请先关闭正在运行的程序", "提示");
                return;
            }
            if(!isSys)
            if (!on&&OpenDate.AddSeconds(1)>DateTime.Now&&OpenDate!=null)
            {
                MessageBox.Show("操作太频繁，请稍后再试 (请至少间隔 1s，防止出现 BUG)", "警告");
                return;
            }
            OpenDate = DateTime.Now;
            try
            {
                if (process != null)
                {
                    if (!process.HasExited)
                    {
                        process.CancelOutputRead();
                        process.CancelErrorRead();
                        process.Kill();
                        Stop();
                        if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
                        return;
                    }
                }
            }catch (Exception ex)
            {
                Logger.Log("[警告] 错误信息：" + ex.Message);
                Logger.Log("[警告] 详细错误信息：" + ex.StackTrace);
                MessageBox.Show("启动报错，请查看是否启动成功，如果失败请尝试点击设置->自动添加安全中心白名单 然后重启软件\n" + ex.StackTrace, "错误");
            }
            
            string server = ServersCombo.Text;
            if(net.servers==null) net.getjson();
            try
            {
                foreach (var item in net.servers)
                {
                    if (item.ServerName == server)
                    {
                        sjson.getjson();
                        sjson.SetServier(item.ServerHost, item.Token);
                        if(item.ServerName != "主节点")
                        {
                            opname = "openp2p21.exe";
                            sjson.config.LogLevel = 2;
                            sjson.Save();
                        }
                        else
                        {
                            opname = "openp2p.exe";
                            sjson.config.LogLevel = 1;
                            sjson.Save();
                        }
                        break;
                    }
                }
            }catch (Exception ex)
            {
                Logger.Log("[警告] 未获取到节点列表" + ex.Message);
            }
            
            string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.exe");
            if (!File.Exists(absolutePath))
            {
                MessageBox.Show("程序文件丢失，无法启动，请从压缩包重新解压 bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单", "警告");
                Logger.Log("[警告] 程序文件丢失，无法启动，请从压缩包重新解压 bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单");
                return;
            }
            else Open();

            openbutton.Content = "关闭";
            Logger.Log("-----------------------程序已开始运行请耐心等待隧道连接----------------------------","提示");
            fstert.Fill = Brushes.Orange;
            on = true;
            fsterto = false;
            Relist();
        }

        public void Open()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", opname);
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            process = new Process();
            process.StartInfo = startInfo;
            
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Dispatcher.Invoke(() =>
                    {
                        richOutput.Text = e.Data + Environment.NewLine + richOutput.Text;
                        Checkopen(e.Data);
                    });
                }
            });
            
            bool tl = true;
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
                                if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
                            }
                            Stop();
                            Logger.Log("主程序 openp2p 崩掉了！请查看软件状态，尝试重新启动，或联系作者","错误");
                            tl = false;
                            _ = DelayAndExecute();
                        }
                    });
                }
            });

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Logger.Log("[错误] 启动失败，看来被安全中心拦截" + ex.ToString());
                MessageBox.Show("启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动\r可以点击本软件设置页面右上角自动添加排除按钮后重试\r内网穿透程序常被黑客用来用来入侵企业内网，故非常容易报毒，请信任程序的安全性\r\r请进行如下操作：Windows 安全中心->病毒和威胁防护->\u201c病毒和威胁防护\u201d设置->管理设置->排除项->添加或删除排除项->添加排除项->文件夹  添加以下路径\r"+ System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin") + "\r"+AppDomain.CurrentDomain.BaseDirectory + "\n\n如果还是不行请进行如下尝试\r\nWindows 安全中心->应用和浏览器控制->智能应用控制设置->关闭", "警告");
                if (process != null) 
                    if(!process.HasExited)
                        process.Kill();
                Stop();
                return;
            }
        }

        private void Stop()
        {
            openbutton.Content = "启动";
            Logger.Log("[提示]----------------------------------程序已停止运行----------------------------------");
            process = null;
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

        public void Checkopen(string m)
        {
            if (m.Contains("autorunApp start"))
            {
                Logger.Log("[提示] 程序启动完毕，请耐心等待隧道连接");
                fstert.Fill = Brushes.Green;
                fsterto = true;
                fsterton = 0;
            }
            if (m.Contains("autorunApp end"))
            {
                Logger.Log("[提示] 程序离线，请检查你的网络设置或查看网络连接是否正常");
                fstert.Fill = Brushes.Orange;
                fsterto = false;
            }
            if (m.Contains("LISTEN ON PORT"))
            {
                string pattern = @"PORT\s+(\w+:\d+)";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string portInfo = match.Groups[1].Value;
                    if (m.Contains("START"))
                    {
                        Logger.Log("[提示] 隧道本地端口为 " + portInfo + " 连接成功");
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
                    }
                    if (m.Contains("END"))
                    {
                        Logger.Log("[错误] 隧道本地端口为 " + portInfo + " 断开连接");
                        state[portInfo] = 1;
                        Relist();
                    }
                }
            }
            if (m.Contains("ERROR P2PNetwork login error"))
            {
                Logger.Log("[错误] 请检查是否连接网络，或是程序是否拥有网络访问权限！");
                state.Clear();
                on = false;
                Relist();
                Strapp();
            }
            if (m.Contains("Only one usage of each socket address"))
            {
                Match match = Regex.Match(m, @"listen\s+(tcp|udp)[\s\S]*?(?:0\.0\.0\.0|\\[::\\]|\\[\\S+\\]|localhost|\\S+):(\d+)");
                if (!match.Success)
                {
                    string pattern = @"(tcp|udp)\s*:\s*(\d+)";
                    match = Regex.Match(m, pattern);
                }

                if (match.Success && fsterto)
                {
                    string protocol = match.Groups[1].Value;
                    string port = match.Groups[2].Value;
                    Logger.Log($"[错误]: 本地端口{protocol}:{port} 被占用，请更换相关本地端口");
                    MessageBox.Show($"本地端口{protocol}:{port} 被占用，请更换相关本地端口！！注意！是连接的创建隧道，开房的仅需在无隧道启用情况下启动！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (on) Strapp();
                }
                else if (!fsterto)
                {
                    fsterton++;
                    if (fsterton > 3)
                    {
                        MessageBox.Show($"注意，你的计算机可能中病毒了！！\r\n请再尝试一次，如果一直这样根据之前的反馈统计情况，如果你每次打开都弹出该窗口，你的计算机极有可能中病毒了，有黑客正在监视你的计算机网络数据，请立即尝试使用杀毒软件\r\n请尝试使用杀毒软件进行全盘查杀，或使用 卡巴斯基病毒清除工具、360 系统急救箱或火绒恶性木马专杀工具\r\n等工具进行查杀", "警告", MessageBoxButton.OK, MessageBoxImage.Hand);
                        if (on) Strapp();
                        return;
                    }

                    if (on) Strapp();
                    string protocol = match.Groups[1].Value;
                    string port = match.Groups[2].Value;
                    Logger.Log($"[错误]: 本地端口{protocol}:{port} 被占用，当前为监听连接端口");
                    if (sjson.config.Network.PublicIPPort.ToString() == port)
                    {
                        if (sjson.config.Network.PublicIPPort < 5000) sjson.config.Network.PublicIPPort = 65000;
                        sjson.config.Network.PublicIPPort = sjson.config.Network.PublicIPPort - 233;
                        sjson.Save();
                    }
                    if (on) Strapp(true);
                }
            }
            if (m.Contains("no such host"))
            {
                Logger.Log("[错误] 请检查 DNS 是否正确，是否连接网络，或是程序是否拥有网络访问权限！");
            }
            if (m.Contains("it will auto reconnect when peer node online"))
            {
                string pattern = @"INFO\s+(\w+)\s+offline";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Logger.Log("[错误]" + id + "不在线！请查询对方 UID 是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
                    MessageBox.Show(id + "不在线！请查询对方 UID 是否输入错误，询问对方程序是否处于启动状态", "警告");
                }
            }
            if (m.Contains("peer offline"))
            {
                Logger.Log("[错误] 你连接的人不在线！请查询对方 UID 是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
            }
            if (m.Contains("NAT type"))
            {
                string pattern = @"NAT type:(\w+)";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string type = match.Groups[1].Value;
                    if (type == "2")
                        Logger.Log("[提示] 你的 NAT 类型为对称形 Symmetric NAT，连接可能受阻，或连接时间较长");
                }

                string pattern2 = @"publicIP:(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
                Match match2 = Regex.Match(m, pattern2);
                if (match2.Success)
                {
                    string ip = match2.Groups[1].Value;
                    Net net = new Net();
                    _ = net.Getisp(ip);
                }
            }
            if (m.Contains("login ok"))
            {
                string pattern = @"node=(\w+)";
                Match match = Regex.Match(m, pattern);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Logger.Log("[提示] 你的实际 UID 为" + id);
                }
            }
        }

        async Task DelayAndExecute()
        {
            Logger.Log("将在 2s 后自动重新启动...");
            await Task.Delay(2000);
            
            Action actionOnMainThread = () =>
            {
                Strapp();
            };

            if (SynchronizationContext.Current != null)
            {
                SynchronizationContext.Current.Post(state => actionOnMainThread(), null);
            }
            else
            {
                actionOnMainThread();
            }
        }
    }
}
