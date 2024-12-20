using CommunityToolkit.Mvvm.Input;
using OplWpf.Models;
using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace OplWpf;

public partial class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        var osVersion = Environment.OSVersion.Version;
        Log.Information("----- OPENP2P Launcher by Guailoudou -----");
        Log.Information("程序启动，当前版本：{@Version}，更新包号：{@Pvn}，系统版本：{Os}", Version, Net.Pvn, osVersion);
    }

    public string Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion ?? "";
        }
    }

    public string DisplayVersion => Version + " - " + Net.Pvn;

    [RelayCommand]
    public void Start()
    {
        if (ConfigManager.Instance.MainState == State.Stop)
        {
            ConfigManager.Instance.Openp2p.Start();
        }
        else
        {
            ConfigManager.Instance.MainState = State.Stop;
        }
    }
}
