using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OplWpf.Models;
using Serilog;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.ViewModels;

public partial class AddViewModel : ObservableObject
{
    public string Name { get; set; } = "自定义";

    public string Uuid { get; set; } = "";

    [ObservableProperty] public partial int SPort { get; set; }

    [ObservableProperty] public partial int CPort { get; set; }

    public string Type { get; set; } = "tcp";

    partial void OnSPortChanged(int value)
    {
        CPort = value;
    }

    [RelayCommand]
    private void AddApp(Window window)
    {
        var config = ConfigManager.Instance.Config;
        Name = Name.Trim();
        Uuid = Uuid.Trim();
        if (config.Network.Node == Uuid)
        {
            Log.Error("自己连自己？");
            MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
            return;
        }

        if (SPort is <= 0 or >= 65536 || CPort is <= 0 or >= 65536)
        {
            MessageBox.Show("端口正常范围为1-65535", "提示");
            return;
        }

        ConfigManager.Instance.AddNewApp(Name, Uuid, SPort, CPort, Type);
        window.Close();
    }
}