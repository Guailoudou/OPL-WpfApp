using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OplWpf.Models;
using OplWpf.Services;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.ViewModels;

[Injection(ServiceLifetime.Transient)]
public partial class TunnelViewModel
{
    private readonly ILogger<TunnelViewModel> _logger;
    private readonly StateProxy _stateProxy;
    private readonly ConfigManager _configManager;
    private readonly DialogService _dialogService;

    public string Node => _configManager.Config.Network.Node;

    public int ShareBandwidth
    {
        get => _configManager.Config.Network.ShareBandwidth;
        set
        {
            _configManager.Config.Network.ShareBandwidth = value;
            _configManager.Save();
        }
    }

    public ObservableCollection<AppViewModel> Apps { get; }

    public TunnelViewModel(ILogger<TunnelViewModel> logger, StateProxy stateProxy, ConfigManager configManager,
        DialogService dialogService)
    {
        _logger = logger;
        _stateProxy = stateProxy;
        _configManager = configManager;
        _dialogService = dialogService;
        var appViewModels = new List<AppViewModel>();
        foreach (var newApp in _configManager.Config.Apps.Select(
                     app => new AppViewModel(app, stateProxy, configManager)
                 ))
        {
            newApp.OnDeleteApp += DeleteApp;
            appViewModels.Add(newApp);
        }

        Apps = [..appViewModels];
        MainWindowViewModel.OnDisableAll += DisableAll;
    }

    [RelayCommand]
    private void CopyUid()
    {
        Clipboard.SetText(Node);
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
                _logger.LogWarning("无法识别的连接码: {connectString}", connectString);
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
            AddApp(newApp);
        }
    }

    [RelayCommand]
    private void Add()
    {
        if (_dialogService.Show() is not { } app) return;
        AddApp(app);
    }

    private void AddApp(AppConfig appConfig)
    {
        if (!_configManager.AddNewApp(appConfig)) return;
        var newApp = new AppViewModel(appConfig, _stateProxy, _configManager);
        newApp.OnDeleteApp += DeleteApp;
        Apps.Add(newApp);
        _logger.LogInformation(
            "创建新的隧道{sUuid}:{sPort}--{type}>>{cPort}",
            appConfig.PeerNode, appConfig.DstPort, appConfig.Protocol, appConfig.SrcPort
        );
    }

    private void DeleteApp(AppViewModel vm)
    {
        Apps.Remove(vm);
    }

    private void DisableAll()
    {
        foreach (var appViewModel in Apps)
        {
            appViewModel.Enabled = 0;
        }

        _configManager.Save();
    }

    [GeneratedRegex(@"^(?:([12]):)?(\w+):(\d+)(?::(\d+))?$")]
    private static partial Regex ConnectString();
}

public partial class AppViewModel : ObservableObject
{
    private readonly StateProxy _stateProxy;
    private readonly ConfigManager _configManager;
    private readonly AppConfig _appConfig;
    public string Name => _appConfig.AppName;
    public string Uid => _appConfig.PeerNode;
    public int DstPort => _appConfig.DstPort;
    public int SrcPort => _appConfig.SrcPort;
    public string Protocol => _appConfig.Protocol;
    public string Address => "127.0.0.1:" + SrcPort;

    [ObservableProperty] public partial int Enabled { get; set; }

    partial void OnEnabledChanged(int value)
    {
        _appConfig.Enabled = value;
    }

    public event Action<AppViewModel>? OnDeleteApp;

    public AppViewModel(AppConfig appConfig, StateProxy stateProxy, ConfigManager configManager)
    {
        _stateProxy = stateProxy;
        _configManager = configManager;
        _appConfig = appConfig;
        Enabled = appConfig.Enabled;
        _stateProxy.PropertyChanged += (_, e) => OnPropertyChanged(e);
        // MainWindowViewModel.OnDisableAll += UpdateEnabled;
    }

    public State AppState => _stateProxy[$"{Protocol}:{SrcPort}"];
    public State MainState => _stateProxy.MainState;

    [RelayCommand]
    private void CopyAddress()
    {
        Clipboard.SetText(Address);
        MessageBox.Show("复制成功，可在游戏中使用CTRL+V粘贴", "提示");
    }

    [RelayCommand]
    private void Delete()
    {
        _configManager.RemoveApp(_appConfig);
        // logger.LogInformation("删除隧道 {appConfig}", _appConfig);
        // MainWindowViewModel.OnDisableAll -= UpdateEnabled;
        OnDeleteApp?.Invoke(this);
    }
}