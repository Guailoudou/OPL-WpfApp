using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace OplWpf.Models;

public class NetworkConfig
{
    public required ulong Token { get; set; }
    public required string Node { get; set; }
    public required string User { get; set; }
    public required int ShareBandwidth { get; set; }
    public required string ServerHost { get; set; }
    public required int ServerPort { get; set; }
    public required int UDPPort1 { get; set; }
    public required int UDPPort2 { get; set; }
    public required int TCPPort { get; set; }
}

public class AppConfig
{
    public required string AppName { get; set; } //隧道名
    public required string Protocol { get; set; } //隧道类型
    public required string Whitelist { get; set; }
    public required int SrcPort { get; set; } //本地端口
    public required string PeerNode { get; set; } //被连uuid
    public required int DstPort { get; set; } //远程端口
    public required string DstHost { get; set; } //远程ip
    public required string PeerUser { get; set; }
    public required string RelayNode { get; set; }
    public required int Enabled { get; set; } //开启？

    [JsonIgnore]
    public int BindingEnabled
    {
        get => Enabled;
        set
        {
            Enabled = value;
            ConfigManager.Instance.Config.Save();
        }
    }

    public override string ToString() => $"{AppName}-{Protocol}-{PeerNode}-{DstPort}->{SrcPort}";
}

public class Config
{
    private static readonly string ConfigFile = Path.Combine(AppContext.BaseDirectory, "bin", "config.json");

    [JsonPropertyName("network")] public required NetworkConfig Network { get; init; }

    [JsonPropertyName("apps")] public required ObservableCollection<AppConfig> Apps { get; init; }

    public int LogLevel { get; set; }

    public static Config Load()
    {
        try
        {
            var jsonString = File.ReadAllText(ConfigFile);
            if (JsonSerializer.Deserialize<Config>(jsonString) is not { } config)
            {
                throw new InvalidDataException("Json格式不正确");
            }

            return config;
        }
        catch (Exception e)
        {
            Log.Error(e, "读取配置文件失败，使用默认配置");
            var config = new Config
            {
                Network = new NetworkConfig
                {
                    Token = 11602319472897248650UL,
                    Node = GenerateUuid(),
                    User = "gldoffice",
                    ShareBandwidth = 10,
                    ServerHost = "api.openp2p.cn",
                    ServerPort = 27183,
                    UDPPort1 = 27182,
                    UDPPort2 = 27183,
                    TCPPort = 50448
                },
                Apps = [],
                LogLevel = 2
            };
            config.Save();
            return config;
        }
    }

    public static string GenerateUuid()
    {
        var bytes = new byte[8]; // 每两个十六进制字符对应一个字节
        Random.Shared.NextBytes(bytes);
        return Convert.ToHexStringLower(bytes); // 转换为十六进制
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, ConfigManager.SerializerOptions);
        File.WriteAllText(ConfigFile, json);
    }

    public void ResetToken()
    {
        Network.Token = 11602319472897248650UL;
        Network.User = "gldoffice";
        LogLevel = 2;
        Save();
    }
}