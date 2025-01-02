using CommunityToolkit.Mvvm.Input;
using System.Windows;
using OplWpf.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;
using OplWpf.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class TunnelViewModel
{
    private readonly ILogger<TunnelViewModel> logger;
    private readonly DialogService dialogService;
    public Config Config { get; }
    public StateManager StateManager { get; }
    public ObservableCollection<AppViewModel> Apps { get; }

    public TunnelViewModel(IOptions<Config> config, ILogger<TunnelViewModel> logger, DialogService dialogService,
        StateManager stateManager)
    {
        this.logger = logger;
        this.dialogService = dialogService;
        Config = config.Value;
        StateManager = stateManager;
        Apps = new(config.Value.Apps.Select(app => new AppViewModel(app)
        {
            DeleteApp = DeleteApp
        }));
    }

    [RelayCommand]
    private void CopyUid()
    {
        Clipboard.SetText(Config.Network.Node);
        MessageBox.Show("复制成功", "提示");
    }

    [RelayCommand]
    private void Import()
    {
        var text = Clipboard.GetText().Trim();
        var connectStrings = new List<(string, string, int, int)>();
        foreach (var connectString in text.Split(';'))
        {
            var match = ConnectString().Match(connectString);
            if (!match.Success)
            {
                logger.LogWarning("无法识别的连接码: {connectString}", connectString);
                MessageBox.Show($"""
                                 无法识别的连接码: {connectString} 
                                 请复制连接码后点击
                                 该功能为一键添加/编辑隧道为连接码隧道，房主可直接编辑发送连接码供连接方使用。 
                                 连接码用法：
                                 [1/2]:uid:端口[:本地端口] --> 1为tcp，2为udp，默认为1 本地端口可省略
                                 示例：1:qwertyuiop:25565:25575
                                 多个连接可以用;间隔同时输入
                                 复制后直接点击该按钮即可完成添加，后直接启动即可
                                 如果确认你复制的符合格式，可尝试点击右边按钮自行添加隧道
                                 """, "错误");
                return;
            }

            var groups = match.Groups;
            var protocol = groups[1].Value == "2" ? "udp" : "tcp";
            var uid = groups[2].Value;
            if (!int.TryParse(groups[3].Value, out var sPort) || sPort is not (> 0 and < 65536))
            {
                MessageBox.Show("端口正常范围为1-65535", "提示");
            }

            if (!int.TryParse(groups[4].Value == "" ? groups[3].Value : groups[4].Value, out var cPort) ||
                cPort is not (> 0 and < 65536))
            {
                MessageBox.Show("端口正常范围为1-65535", "提示");
            }
            connectStrings.Add(new(protocol, uid, sPort, cPort));
        }

        foreach (var (protocol, uid, sPort, cPort) in connectStrings)
        {
            var newApp = new AppConfig("自定义", protocol, cPort, uid, sPort);
            if (Config.AddNewApp(newApp))
            {
                Apps.Add(new AppViewModel(newApp));
            }
        }
    }

    [RelayCommand]
    private void AddApp()
    {
        var newApp = new AppConfig("自定义", "tcp", 0, "", 0);
        if (dialogService.Show(new AddViewModel(newApp)) == true)
        {
            Config.AddNewApp(newApp);
            Apps.Add(new AppViewModel(newApp) { DeleteApp = DeleteApp });
            logger.LogInformation(
                "创建新的隧道{sUuid}:{sPort}--{type}>>{cPort}",
                newApp.PeerNode, newApp.DstPort, newApp.Protocol, newApp.SrcPort
            );
        }
    }

    private void DeleteApp(AppViewModel vm)
    {
        Config.RemoveApp(vm.AppConfig);
        Apps.Remove(vm);
        logger.LogInformation("删除隧道 {appConfig}", vm.AppConfig);
    }

    [GeneratedRegex(@"^(?:([12]):)?(\w+):(\d+)(?::(\d+))?$")]
    private static partial Regex ConnectString();
}

public partial class AppViewModel : ObservableObject
{
    private readonly StateManager stateManager;
    public AppConfig AppConfig { get; }
    public string Name => AppConfig.AppName;
    public string Uid => AppConfig.PeerNode;
    public int DstPort => AppConfig.DstPort;
    public int SrcPort => AppConfig.SrcPort;
    public string Protocol => AppConfig.Protocol;
    public string Address => "127.0.0.1:" + SrcPort;

    public int Enabled
    {
        get => AppConfig.Enabled;
        set
        {
            AppConfig.Enabled = value;
            App.GetService<IOptions<Config>>().Value.Save();
        }
    }

    public AppViewModel(AppConfig appConfig)
    {
        stateManager = App.GetService<StateManager>();
        stateManager.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainState))
            {
                OnPropertyChanged(nameof(MainState));
            }

            if (e.PropertyName == nameof(stateManager.AppState))
            {
                OnPropertyChanged(nameof(State));
            }
        };
        AppConfig = appConfig;
        App.GetService<IMessenger>().Register<DisableAllMessage>(this, (_, _) => OnPropertyChanged(nameof(Enabled)));
    }

    public State State => stateManager[Protocol + ':' + SrcPort];
    public State MainState => stateManager.MainState;

    public Action<AppViewModel>? DeleteApp { get; set; }

    [RelayCommand]
    private void CopyAddress()
    {
        Clipboard.SetText(Address);
        MessageBox.Show("复制成功，可在游戏中使用CTRL+V粘贴", "提示");
    }

    [RelayCommand]
    private void Delete() => DeleteApp?.Invoke(this);
}