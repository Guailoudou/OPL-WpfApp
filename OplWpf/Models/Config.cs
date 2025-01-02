using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OplWpf.Models;

public class NetworkConfig(string node)
{
    public ulong Token { get; set; } = 11602319472897248650UL;
    public string Node { get; set; } = node;
    public string User { get; set; } = "gldoffice";
    public int ShareBandwidth { get; set; } = 10;
    public string ServerHost { get; set; } = "api.openp2p.cn";
    public int ServerPort { get; set; } = 27183;
    public int UDPPort1 { get; set; } = 27182;
    public int UDPPort2 { get; set; } = 27183;
    public int TCPPort { get; set; } = 50448;
}

public class AppConfig(string appName, string protocol, int srcPort, string peerNode, int dstPort)
{
    public string AppName { get; set; } = appName; //隧道名
    public string Protocol { get; set; } = protocol; //隧道类型
    public string Whitelist { get; set; } = "";
    public int SrcPort { get; set; } = srcPort; //本地端口
    public string PeerNode { get; set; } = peerNode; //被连uuid
    public int DstPort { get; set; } = dstPort;//远程端口
    public string DstHost { get; set; } = "localhost"; //远程ip
    public string PeerUser { get; set; } = "";
    public string RelayNode { get; set; } = "";
    public int Enabled { get; set; } = 1;//开启？

    public override string ToString() => $"{AppName}-{Protocol}-{PeerNode}-{DstPort}->{SrcPort}";
}

public class Config
{
    public static readonly string ConfigFile = Path.Combine(AppContext.BaseDirectory, "bin", "config.json");

    [JsonPropertyName("network")]
    public NetworkConfig Network { get; set; } = new NetworkConfig(GenerateUuid());

    [JsonPropertyName("apps")] public HashSet<AppConfig> Apps { get; set; } = [];

    public int LogLevel { get; set; } = 2;

    private readonly JsonSerializerOptions serializerOptions = App.GetService<JsonSerializerOptions>();

    public bool AddNewApp(AppConfig appConfig)
    {
        var res = Apps.Add(appConfig);
        Save();
        return res;
    }

    public void RemoveApp(AppConfig appConfig)
    {
        Apps.Remove(appConfig);
        Save();
    }

    public static string GenerateUuid()
    {
        var bytes = new byte[8]; // 每两个十六进制字符对应一个字节
        Random.Shared.NextBytes(bytes);
        return Convert.ToHexStringLower(bytes); // 转换为十六进制
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, serializerOptions);
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