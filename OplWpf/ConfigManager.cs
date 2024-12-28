using CommunityToolkit.Mvvm.ComponentModel;
using OplWpf.Models;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace OplWpf;

public partial class ConfigManager : ObservableObject
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<ConfigManager> instance = new(() => new ConfigManager());

    public static ConfigManager Instance => instance.Value;

    public Update Update { get; set; } = new();

    [ObservableProperty] public partial State MainState { get; set; }

    public Config Config { get; }

    public Setting Setting { get; }

    public Openp2p Openp2p { get; } = new();

    public Dictionary<string, State> AppState { get; } = [];

    public List<TcpClientWithKeepAlive> Tcps { get; } = [];

    public List<UdpClientKeepAlive> Udps { get; } = [];

    private ConfigManager()
    {
        Update = new Update();
        MainState = State.Stop;
        Config = Config.Load();
        Setting = Setting.Load();
    }

    public void AddNewApp(string appName, string sUuid, int sPort, int cPort, string type)
    {
        Config.Apps.Add(new AppConfig
        {
            AppName = appName,
            PeerNode = sUuid,
            Whitelist = "",
            Protocol = type,
            SrcPort = cPort,
            DstPort = sPort,
            DstHost = "localhost",
            Enabled = 1,
            PeerUser = "",
            RelayNode = ""
        });
        Log.Information("创建新的隧道{sUuid}:{sPort}--{type}>>{cPort}", sUuid, sPort, type, cPort);
        Config.Save();
    }

    [RelayCommand]
    private void RemoveApp(AppConfig appConfig)
    {
        Config.Apps.Remove(appConfig);
        Log.Information("删除隧道 {app}", appConfig);
        Config.Save();
    }

    public void AppStateChanged()
    {
        OnPropertyChanged(nameof(AppState));
    }
}