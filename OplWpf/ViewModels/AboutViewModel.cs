using System.Diagnostics;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace OplWpf.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty] public partial string DaySay { get; set; } = "联网获取中";

    public AboutViewModel()
    {
        GetDaySayAsync().ContinueWith(s => DaySay = s.Result);
    }

    private async Task<string> GetDaySayAsync()
    {
        var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync("https://uapis.cn/api/say");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "获取失败";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "获取每日一句失败");
            return "获取失败";
        }
    }

    [RelayCommand]
    private async Task RefreshDaySay()
    {
        DaySay = await GetDaySayAsync();
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
}