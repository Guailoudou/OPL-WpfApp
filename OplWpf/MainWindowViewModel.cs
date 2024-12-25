using CommunityToolkit.Mvvm.Input;
using OplWpf.Models;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OplWpf;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        var osVersion = Environment.OSVersion.Version;
        var fileName = Process.GetCurrentProcess().MainModule?.FileName;
        if (fileName != null)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
            Version = fileVersionInfo.FileVersion ?? "";
        }

        Log.Information("----- OPENP2P Launcher by Guailoudou -----");
        Log.Information("程序启动，当前版本：{@Version}，更新包号：{@Pvn}，系统版本：{Os}", Version, Net.Pvn, osVersion);
    }


    private string Version { get; } = "";

    public string DisplayVersion => Version + " - " + Net.Pvn;

    public string ButtonText => ConfigManager.Instance.MainState == State.Stop
        ? "启动"
        : "停止";

    [RelayCommand]
    private async Task Start()
    {
        if (ConfigManager.Instance.MainState == State.Stop)
        {
            await ConfigManager.Instance.Openp2p.Start();
        }
        else
        {
            ConfigManager.Instance.Openp2p.Stop();
        }

        OnPropertyChanged(nameof(ButtonText));
    }
}