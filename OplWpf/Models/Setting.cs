using Serilog;
using System.IO;
using System.Text.Json;

namespace OplWpf.Models;

public class Setting
{
    private static readonly string SettingFile = Path.Combine(AppContext.BaseDirectory, "bin", "set.json");

    public string Color { get; set; } = "#FF0078D4"; // 颜色
    public string Theme { get; set; } = ""; // 主题("Light" 或 "Dark")
    public string CsProduct { get; set; } = "4C4C4544-0054-4710-8054-C3C04F315A33"; //BIOSUUID
    public bool AutoUpop { get; set; } = true; // 自动更新openp2p
    public bool AutoUp { get; set; } = true; // 自动更新
    public bool AutoOpen { get; set; } = false; //运行后自动启动
    public bool IspWarning { get; set; } = true; // 获取isp

    public static Setting Load()
    {
        try
        {
            var jsonString = File.ReadAllText(SettingFile);
            return JsonSerializer.Deserialize<Setting>(jsonString)
                ?? throw new InvalidDataException("Json格式不正确");
        }
        catch (Exception e)
        {
            Log.Error(e, "读取设置文件失败，使用默认设置");
            var setting = new Setting();
            setting.Save();
            return setting;
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, ConfigManager.SerializerOptions);
        File.WriteAllText(SettingFile, json);
    }
}