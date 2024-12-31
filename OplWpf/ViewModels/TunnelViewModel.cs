using CommunityToolkit.Mvvm.Input;
using System.Windows;
using OplWpf.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class TunnelViewModel(IOptions<Config> config, ILogger<TunnelViewModel> logger, StateManager stateManager)
{
    public Config Config { get; } = config.Value;
    public StateManager StateManager { get; } = stateManager;

    [RelayCommand]
    private void CopyUid()
    {
        Clipboard.SetText(Config.Network.Node);
        MessageBox.Show("复制成功", "提示");
    }

    [RelayCommand]
    private void DeleteApp(AppConfig app)
    {
        Config.RemoveApp(app);
        logger.LogInformation("删除隧道 {app}", app);
    }
}