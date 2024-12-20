using iNKORE.UI.WPF.Modern.Controls;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.Models;

public class Openp2p
{
    private static readonly string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.exe");

    private readonly Process process;
    public Openp2p()
    {
        process = new Process
        {
            StartInfo =
            {
                FileName = exePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.OutputDataReceived += (sender, e) =>
        {
            if (e is { Data: string { Length: > 0 } msg })
            {
                Log.Information(msg);
                CheckMessage(msg);
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e is { Data: string { Length: > 0 } msg })
            {
                Log.Error(msg);
            }
        };
    }

    public void Start()
    {
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            ConfigManager.Instance.MainState = State.Loading;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "启动失败，看来被安全中心拦截");
            MessageBox.Show($"""
                            启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动
                            内网穿透程序常被黑客用来用来入侵企业内网，故非常容易报毒，请信任程序的安全性
                            请进行如下操作：Windows安全中心->病毒和威胁防护->“病毒和威胁防护”设置->管理设置->排除项->添加或删除排除项->添加排除项->文件夹  添加以下路径
                            {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")}
                            {AppDomain.CurrentDomain.BaseDirectory}
                            如果还是不行请进行如下尝试
                            Windows安全中心->应用和浏览器控制->智能应用控制设置->关闭
                            """, "警告");
        }
    }

    public void Stop()
    {
        if (!process.HasExited)
        {
            process.CancelOutputRead();
            process.CancelErrorRead();
            process.Kill();
        }
        //if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
        //if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    public void CheckMessage(string message)
    {
        if (message.Contains("autorunApp start"))
        {
            Log.Information("程序启动完毕，请耐心等待隧道连接"); //启动完毕
            ConfigManager.Instance.MainState = State.Running;
        }
        if (message.Contains("autorunApp end"))
        {
            Log.Information("程序离线，请检查你的网络设置或查看网络连接是否正常");
            ConfigManager.Instance.MainState = State.Loading;
        }
        if (message.Contains("LISTEN ON PORT")) //连接成功or断开
        {
            var match = Regex.Match(message, @"PORT\s+(\w+:\d+)");
            if (match.Success)
            {
                var portInfo = match.Groups[1].Value;
                if (message.Contains("START"))
                {
                    Log.Information("隧道本地端口为 {portInfo} 连接成功", portInfo);
                    ConfigManager.Instance.AppState[portInfo] = State.Running;
                    string[] parts = portInfo.Split(':');
                    string type = parts[0];
                    int port = int.Parse(parts[1]);
                    if (type == "tcp")
                    {
                        ConfigManager.Instance.Tcps.Add(new("127.0.0.1", port));
                        //if (!Multicast.IsMulticastOpen() && tcpnum == 1)
                        //{
                        //    Multicast.SetSrcPort(port);
                        //    _ = Multicast.Seed();
                        //}

                    }
                    //else
                    //{
                    //    udps.Add(new UdpClientKeepAlive("127.0.0.1", port));
                    //}
                }
                if (message.Contains("END"))
                {
                    Log.Error("隧道本地端口为 {portInfo} 断开连接", portInfo);
                    ConfigManager.Instance.AppState[portInfo] = State.Loading;
                }
            }
        }
        if (message.Contains("ERROR P2PNetwork login error"))
        {
            Log.Error("请检查是否连接网络，或是程序是否拥有网络访问权限！");
            ConfigManager.Instance.AppState.Clear();
            ConfigManager.Instance.MainState = State.Stop;
            Start();
        }
        if (message.Contains("Only one usage of each socket address"))
        {
            var match = Regex.Match(message, @"(tcp|udp)\s*:\s*(\d+)");

            if (match.Success && ConfigManager.Instance.MainState == State.Running)
            {
                // 提取并输出匹配到的协议和端口号
                string protocol = match.Groups[1].Value;
                string port = match.Groups[2].Value;
                Log.Error("本地端口{protocol}:{port}被占用，请更换相关本地端口", protocol, port);
                MessageBox.Show($"本地端口{protocol}:{port}被占用，请更换相关本地端口！！注意！是连接的创建隧道，开房的仅续在无隧道启用情况下启动！！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //if (on) Strapp();
            }
            else if (ConfigManager.Instance.MainState == State.Loading)
            {
                MessageBox.Show("""
                                注意，你的计算机可能中病毒了！！
                                根据之前的反馈统计情况，如果你每次打开都弹出该窗口，你的计算机极有可能中病毒了，有黑客正在监视你的计算机网络数据，请立即尝试使用杀毒软件
                                请尝试使用杀毒软件进行全盘查杀，或使用 卡巴斯基病毒清除工具、360系统急救箱或火绒恶性木马专杀工具
                                等工具进行查杀
                                """, "警告", MessageBoxButton.OK, MessageBoxImage.Hand);
                //if (on) Strapp();
            }
        }
        if (message.Contains("no such host"))
        {
            Log.Error("请检查DNS是否正确，是否连接网络，或是程序是否拥有网络访问权限！");
        }
        if (message.Contains("it will auto reconnect when peer node online"))//对方不在线
        {
            var match = Regex.Match(message, @"INFO\s+(\w+)\s+offline");
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                Log.Error("{id}不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接", id);
                MessageBox.Show(id + "不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态", "警告");
            }
        }
        if (message.Contains("peer offline"))//对方不在线
        {

            Log.Error("你连接的人不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
            //MessageBox.Show("你连接的人不在线！不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态", "警告");

        }
        if (message.Contains("NAT type"))
        {
            var match = Regex.Match(message, @"NAT type:(\w+)");
            if (match.Success)
            {
                string type = match.Groups[1].Value;
                //Logger.Log("[提示]你的NAT类型为"+type);
                if (type == "2")
                {
                    Log.Information("你的NAT类型为对称形 Symmetric NAT，连接可能受阻，或连接时间较长");
                }
            }

            var match2 = Regex.Match(message, @"publicIP:(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
            if (match2.Success)
            {
                string ip = match2.Groups[1].Value;
                //Logger.Log("[提示]公网IP为" + ip);
                _ = Net.GetIsp(ip);
            }
        }
        if (message.Contains("login ok")) //登录中心成功
        {
            Match match = Regex.Match(message, @"node=(\w+)");
            Match umatch = Regex.Match(message, @"user=(\w+)");
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                Log.Information("你的实际UID为{id}", id);
            }
            if (umatch.Success)
            {
                string user = umatch.Groups[1].Value;
                if (user != "gldoffice")
                {

                    Log.Error("疑似token丢失，开始自动尝试修复");
                    //MessageBox.Show("疑似token丢失，已自动尝试修复", "严重错误");
                    //Strapp();
                    ConfigManager.Instance.Config.ResetToken();
                    //sjson.ReSetToken(); //修复token
                    Log.Information("尝试修复完毕");
                    Restart();
                }
            }
        }
    }
}
