﻿using Serilog;
using System.IO;
using System.Text.Json;

namespace OplWpf.Models;

public class Setting
{
    private static readonly string settingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "set.json");

    public string Color { get; set; } // 颜色
    public string Theme { get; set; } // 主题("Light" 或 "Dark")
    public string CsProduct { get; set; } //BIOSUUID
    public bool AutoUpop { get; set; } = true; // 自动更新openp2p
    public bool AutoUp { get; set; } = true;  // 自动更新
    public bool AutoOpen { get; set; } = false; //运行后自动启动
    public bool IspWarning { get; set; } = true; // 获取isp

    public Setting()
    {
        Color = "#FF0078D4";
        Theme = "";
        CsProduct = "4C4C4544-0054-4710-8054-C3C04F315A33";
    }

    public static Setting Load()
    {
        try
        {
            var jsonString = File.ReadAllText(settingFile);
            if (JsonSerializer.Deserialize<Setting>(jsonString) is not Setting setting)
            {
                throw new InvalidDataException("Json格式不正确");
            }
            else
            {
                return setting;
            }
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
        File.WriteAllText(settingFile, json);
    }
}