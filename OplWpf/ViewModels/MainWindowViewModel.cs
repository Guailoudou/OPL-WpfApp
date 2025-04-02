using CommunityToolkit.Mvvm.Input;
using OplWpf.Models;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommunityToolkit.Mvvm.Messaging;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ConfigManager _configManager;
    private readonly Openp2p _openP2P;
    public StateProxy StateProxy { get; }

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, ConfigManager configManager, Openp2p openP2P,
        StateProxy stateProxy)
    {
        _configManager = configManager;
        _openP2P = openP2P;
        StateProxy = stateProxy;
        var osVersion = Environment.OSVersion.Version;
        var fileName = Process.GetCurrentProcess().MainModule?.FileName;
        if (fileName != null)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
            Version = fileVersionInfo.FileVersion ?? "";
        }

        logger.LogInformation("----- OPENP2P Launcher by Guailoudou -----");
        logger.LogInformation("程序启动，当前版本：{Version}，更新包号：{Pvn}，系统版本：{Os}", Version, Net.Pvn, osVersion);
    }


    private string Version { get; } = "";

    public string DisplayVersion => Version + " - " + Net.Pvn;

    [ObservableProperty] public partial string ButtonText { get; set; } = "启动";

    [RelayCommand]
    private void DisableAll()
    {
        OnDisableAll?.Invoke();
    }

    public static event Action? OnDisableAll;

    [RelayCommand]
    private async Task Start()
    {
        if (StateProxy.MainState == State.Stop)
        {
            await _openP2P.Start();
            ButtonText = "停止";
        }
        else
        {
            _openP2P.Stop();
            ButtonText = "启动";
        }
    }
}