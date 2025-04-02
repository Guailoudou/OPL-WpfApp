using Microsoft.Extensions.Hosting;
using OplWpf.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace OplWpf.Services;

public partial class UpdateService(Update update) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string url = "https://file.gldhn.top/file/json/preset.json";
        var httpClient = new HttpClient();
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var response = await httpClient.GetAsync(url, stoppingToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var info = await response.Content.ReadFromJsonAsync<LatestInfo>(stoppingToken);
                        if (info?.UpLog != null)
                        {
                            update.UpdateLog = info.UpLog;
                        }
                    }
                }
                catch
                {
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}