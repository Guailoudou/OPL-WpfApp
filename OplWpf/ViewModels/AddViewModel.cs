using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OplWpf.Models;

namespace OplWpf.ViewModels;

public partial class AddViewModel(AppConfig app) : ObservableObject
{
    public string Name
    {
        get => app.AppName;
        set => app.AppName = value;
    }

    public string Uuid
    {
        get => app.PeerNode;
        set => app.PeerNode = value;
    }

    public int SPort
    {
        get => app.DstPort;
        set
        {
            app.DstPort = value;
            app.SrcPort = value;
            OnPropertyChanged(nameof(CPort));
        }
    }

    public int CPort
    {
        get => app.SrcPort;
        set => app.SrcPort = value;
    }

    public string Type
    {
        get => app.Protocol;
        set => app.Protocol = value;
    }

    public Action? CloseWindow { get; set; }

    [RelayCommand]
    private void AddApp()
    {
        var config = App.GetService<IOptions<Config>>().Value;
        var logger = App.GetService<ILogger<AddViewModel>>();

        Name = Name.Trim();
        Uuid = Uuid.Trim();
        if (string.IsNullOrEmpty(Uuid))
        {
            MessageBox.Show("UUID不能为空", "错误");
            return;
        }

        if (config.Network.Node == Uuid)
        {
            logger.LogError("自己连自己？");
            MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
            return;
        }

        if (SPort is not (> 0 and < 65536) || CPort is not (> 0 and < 65536))
        {
            MessageBox.Show("端口正常范围为1-65535", "提示");
            return;
        }

        if (config.Apps.Contains(app))
        {
            MessageBox.Show("不能添加相同的连接", "提示");
            return;
        }
        CloseWindow?.Invoke();
    }
}