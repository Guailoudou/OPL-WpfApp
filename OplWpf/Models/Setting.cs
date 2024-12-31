using System.IO;
using System.Text.Json;

namespace OplWpf.Models;

public class Setting
{
    public static readonly string SettingFile = Path.Combine(AppContext.BaseDirectory, "bin", "set.json");

    public string Color { get; set; } = "#FF0078D4"; // 颜色
    public string Theme { get; set; } = ""; // 主题("Light" 或 "Dark")
    public string CsProduct { get; set; } = "4C4C4544-0054-4710-8054-C3C04F315A33"; //BIOSUUID
    public bool AutoUpop { get; set; } = true; // 自动更新openp2p
    public bool AutoUp { get; set; } = true; // 自动更新
    public bool AutoOpen { get; set; } = false; //运行后自动启动
    public bool IspWarning { get; set; } = true; // 获取isp

    private readonly JsonSerializerOptions serializerOptions;

    public Setting(JsonSerializerOptions serializerOptions)
    {
        this.serializerOptions = serializerOptions;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, serializerOptions);
        File.WriteAllText(SettingFile, json);
    }
}