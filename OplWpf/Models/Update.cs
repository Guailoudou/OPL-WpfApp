using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;

namespace OplWpf.Models;

public class Preset
{
}

public class LatestInfo
{
    public required IReadOnlyList<Preset> Presets { get; set; }
    public required int Version { get; set; }
    public required string UpLog { get; set; }
    public required string UpUrl { get; set; }
    public required string UpHash { get; set; }
    public required string OpUrl { get; set; }
    public required string OpHash { get; set; }
}

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public partial class Update : ObservableObject
{
    [ObservableProperty]
    public partial string UpdateLog { get; set; } = "联网获取中";
}
