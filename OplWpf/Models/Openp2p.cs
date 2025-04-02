using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.Models;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public partial class Openp2p
{
    private static readonly string ExePath = Path.Combine(AppContext.BaseDirectory, "bin", "openp2p.exe");

    private Process? _process;
    private readonly ILogger<Openp2p> _logger;
    private readonly ConfigManager _configManager;
    private readonly StateProxy _stateProxy;
    private readonly HeartBeat _heartBeat;

    public Openp2p(ILogger<Openp2p> logger,
        ConfigManager configManager,
        StateProxy stateProxy,
        HeartBeat heartBeat,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _configManager = configManager;
        _stateProxy = stateProxy;
        _heartBeat = heartBeat;
        appLifetime.ApplicationStopping.Register(Stopping);
    }


    public async Task Start()
    {
        if (!File.Exists(ExePath))
        {
            await MessageBox.ShowAsync("程序文件丢失，无法启动，请从压缩包重新解压bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单", "警告");
            _logger.LogWarning("程序文件丢失，无法启动，请从压缩包重新解压bin/openp2p.exe 文件可能被杀毒删了，请为程序目录添加白名单");
            return;
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = ExePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var progress = new Progress<RaiseMessage>(ShowMessageBox);
        process.OutputDataReceived += (_, e) =>
        {
            if (e is not { Data: { Length: > 0 } msg }) return;
            _logger.LogInformation("{}", msg);
            CheckMessage(msg, progress);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e is { Data: { Length: > 0 } msg })
            {
                _logger.LogError("{}", msg);
            }
        };
        try
        {
            await Task.Run(process.Start);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _process = process;

            _stateProxy.MainState = State.Loading;
            foreach (var app in _configManager.Config.Apps.Where(app => app.Enabled != 0))
            {
                _stateProxy[app.Protocol + ':' + app.SrcPort] = State.Loading;
            }

            _logger.LogInformation("-----------------------程序已开始运行请耐心等待隧道连接----------------------------");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动失败，看来被安全中心拦截");
            await MessageBox.ShowAsync($"""
                                        启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动
                                        内网穿透程序常被黑客用来用来入侵企业内网，故非常容易报毒，请信任程序的安全性
                                        请进行如下操作：Windows安全中心->病毒和威胁防护->“病毒和威胁防护”设置->管理设置->排除项->添加或删除排除项->添加排除项->文件夹  添加以下路径
                                        {Path.Combine(AppContext.BaseDirectory, "bin")}
                                        {AppContext.BaseDirectory}
                                        如果还是不行请进行如下尝试
                                        Windows安全中心->应用和浏览器控制->智能应用控制设置->关闭
                                        """, "警告");
        }
    }

    public void Stop()
    {
        Stopping();
        _heartBeat.ClearUdp();
        _heartBeat.ClearTcp();
        _stateProxy.MainState = State.Stop;
        _stateProxy.Clear();
        _logger.LogInformation("----------------------------------程序已停止运行----------------------------------");
    }

    private void Stopping()
    {
        if (_process is not { HasExited: false }) return;
        _process.CancelOutputRead();
        _process.CancelErrorRead();
        _process.Kill();
        _process.Dispose();
        _process = null;
    }

    public void Restart()
    {
        Stop();
        Start().GetAwaiter().GetResult();
    }

    public void ShowMessageBox(RaiseMessage msg)
    {
        if (msg.Icon is { } icon)
        {
            MessageBox.Show(msg.Message, msg.Caption, msg.Button, icon);
        }
        else
        {
            MessageBox.Show(msg.Message, msg.Caption, msg.Button);
        }
    }

    public void CheckMessage(string message, IProgress<RaiseMessage> progress)
    {
        if (message.Contains("autorunApp start"))
        {
            _logger.LogInformation("程序启动完毕，请耐心等待隧道连接"); //启动完毕
            _stateProxy.MainState = State.Running;
        }

        if (message.Contains("autorunApp end"))
        {
            _logger.LogInformation("程序离线，请检查你的网络设置或查看网络连接是否正常");
            _stateProxy.MainState = State.Loading;
        }

        if (message.Contains("LISTEN ON PORT")) //连接成功or断开
        {
            var match = PortRegex().Match(message);
            if (match.Success)
            {
                var portInfo = match.Groups[1].Value;
                if (message.Contains("START"))
                {
                    _logger.LogInformation("隧道本地端口为 {portInfo} 连接成功", portInfo);
                    _stateProxy[portInfo] = State.Running;
                    var parts = portInfo.Split(':');
                    var type = parts[0];
                    var port = int.Parse(parts[1]);
                    if (type == "tcp")
                    {
                        _heartBeat.AddTcp("127.0.0.1", port);
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
                    _logger.LogError("隧道本地端口为 {portInfo} 断开连接", portInfo);
                    _stateProxy[portInfo] = State.Loading;
                }
            }
        }

        if (message.Contains("ERROR P2PNetwork login error"))
        {
            _logger.LogError("请检查是否连接网络，或是程序是否拥有网络访问权限！");
            _stateProxy.Clear();
            _stateProxy.MainState = State.Stop;
            Restart();
        }

        if (message.Contains("Only one usage of each socket address"))
        {
            var match = SocketRegex().Match(message);

            if (match.Success && _stateProxy.MainState == State.Running)
            {
                // 提取并输出匹配到的协议和端口号
                var protocol = match.Groups[1].Value;
                var port = match.Groups[2].Value;
                _logger.LogError("本地端口{protocol}:{port}被占用，请更换相关本地端口", protocol, port);
                progress.Report(new RaiseMessage
                {
                    Message = $"本地端口{protocol}:{port}被占用，请更换相关本地端口！！注意！是连接的创建隧道，开房的仅续在无隧道启用情况下启动！！",
                    Caption = "错误",
                    Icon = MessageBoxImage.Error,
                });
                Stop();
            }
            else if (_stateProxy.MainState == State.Loading)
            {
                progress.Report(new RaiseMessage
                {
                    Message = """
                              注意，你的计算机可能中病毒了！！
                              根据之前的反馈统计情况，如果你每次打开都弹出该窗口，你的计算机极有可能中病毒了，有黑客正在监视你的计算机网络数据，请立即尝试使用杀毒软件
                              请尝试使用杀毒软件进行全盘查杀，或使用 卡巴斯基病毒清除工具、360系统急救箱或火绒恶性木马专杀工具
                              等工具进行查杀
                              """,
                    Caption = "警告",
                    Icon = MessageBoxImage.Hand,
                });
                Stop();
            }
        }

        if (message.Contains("no such host"))
        {
            _logger.LogError("请检查DNS是否正确，是否连接网络，或是程序是否拥有网络访问权限！");
        }

        if (message.Contains("it will auto reconnect when peer node online")) //对方不在线
        {
            var match = OfflineRegex().Match(message);
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                _logger.LogError("{id}不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接", id);
                progress.Report(new RaiseMessage
                {
                    Message = id + "不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态",
                    Caption = "警告"
                });
            }
        }

        if (message.Contains("peer offline")) //对方不在线
        {
            _logger.LogError("你连接的人不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态，当对方在线时会自动进行连接");
            //MessageBox.Show("你连接的人不在线！不在线！请查询对方UID是否输入错误，询问对方程序是否处于启动状态", "警告");
        }

        if (message.Contains("NAT type"))
        {
            var match = NatRegex().Match(message);
            if (match.Success)
            {
                var type = match.Groups[1].Value;
                //Logger.Log("[提示]你的NAT类型为"+type);
                if (type == "2")
                {
                    _logger.LogInformation("你的NAT类型为对称形 Symmetric NAT，连接可能受阻，或连接时间较长");
                }
            }

            var match2 = IpRegex().Match(message);
            if (match2.Success)
            {
                var ip = match2.Groups[1].Value;
                //Logger.Log("[提示]公网IP为" + ip);
                Task.Run(async () => await Net.GetIsp(ip));
            }
        }

        if (message.Contains("login ok")) //登录中心成功
        {
            var match = NodeRegex().Match(message);
            var umatch = UserRegex().Match(message);
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                _logger.LogInformation("你的实际UID为{id}", id);
            }

            if (!umatch.Success) return;
            var user = umatch.Groups[1].Value;
            if (user == "gldoffice") return;
            _logger.LogError("疑似token丢失，开始自动尝试修复");
            //MessageBox.Show("疑似token丢失，已自动尝试修复", "严重错误");
            //Strapp();
            _configManager.ResetToken();
            //sjson.ReSetToken(); //修复token
            _logger.LogInformation("尝试修复完毕");
            Restart();
        }
    }

    public class RaiseMessage
    {
        public required string Message { get; set; }
        public required string Caption { get; set; }
        public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;
        public MessageBoxImage? Icon { get; set; } = null;
    }

    [GeneratedRegex(@"PORT\s+(\w+:\d+)")]
    private static partial Regex PortRegex();

    [GeneratedRegex(@"(tcp|udp)\s*:\s*(\d+)")]
    private static partial Regex SocketRegex();

    [GeneratedRegex(@"INFO\s+(\w+)\s+offline")]
    private static partial Regex OfflineRegex();

    [GeneratedRegex(@"NAT type:(\w+)")]
    private static partial Regex NatRegex();

    [GeneratedRegex(@"publicIP:(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})")]
    private static partial Regex IpRegex();

    [GeneratedRegex(@"node=(\w+)")]
    private static partial Regex NodeRegex();

    [GeneratedRegex(@"user=(\w+)")]
    private static partial Regex UserRegex();
}