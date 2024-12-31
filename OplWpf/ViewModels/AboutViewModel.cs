using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OplWpf.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty] public partial string DaySay { get; set; } = "联网获取中";
    [ObservableProperty] public partial IReadOnlyList<Thank> ThankList { get; set; } = [];

    public Update Update { get; }

    public AboutViewModel(Update update)
    {
        Update = update;
        Net.GetDaySayAsync().ContinueWith(s => DaySay = s.Result);
        Net.GetThankListAsync().ContinueWith(t => ThankList = t.Result);
    }

    [RelayCommand]
    private void OpenWiki()
    {
        Process.Start("explorer.exe", "https://blog.gldhn.top/2024/04/19/opl_ui/");
    }

    [RelayCommand]
    private void OpenMe()
    {
        Process.Start("explorer.exe", "https://space.bilibili.com/496960407");
    }

    [RelayCommand]
    private void OpenGit()
    {
        Process.Start("explorer.exe", "https://github.com/Guailoudou/OPL-WpfApp");
    }

    [RelayCommand]
    private async Task RefreshDaySay()
    {
        DaySay = await Net.GetDaySayAsync();
    }

    [RelayCommand]
    private void CopyDaySay()
    {
        Clipboard.SetText(DaySay);
        MessageBox.Show("复制成功");
    }
}