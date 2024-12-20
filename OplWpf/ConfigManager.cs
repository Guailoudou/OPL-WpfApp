using CommunityToolkit.Mvvm.ComponentModel;
using OplWpf.Models;
using System.Text.Json;

namespace OplWpf;

public partial class ConfigManager : ObservableObject
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true
    };

    private static readonly Lazy<ConfigManager> instance = new(() => new ConfigManager());

    public static ConfigManager Instance => instance.Value;

    [ObservableProperty]
    public partial State MainState { get; set; }

    public Config Config { get; }

    public Setting Setting { get; }

    public Openp2p Openp2p { get; } = new();

    public Dictionary<string, State> AppState { get; } = [];

    public List<TcpClientWithKeepAlive> Tcps { get; } = [];

    public List<UdpClientKeepAlive> Udps { get; } = [];

    private ConfigManager()
    {
        MainState = State.Stop;
        Config = Config.Load();
        Setting = Setting.Load();
    }
}
