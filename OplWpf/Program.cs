using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OplWpf;
using OplWpf.Models;
using OplWpf.Services;
using OplWpf.Views;
using Serilog;

if (!Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA))
{
    Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
}

var host = new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile(Config.ConfigFile, true)
            .AddJsonFile(Setting.SettingFile, true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        });
        services.Configure<Config>(context.Configuration);
        services.Configure<Setting>(context.Configuration);
        services.AddInjections();

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddHostedService<WpfHostedService<App, MainWindow>>();
        services.AddHostedService<HeartBeatService>();
        services.AddHostedService<UpdateService>();
    })
    .ConfigureLogging((_, logger) =>
    {
        const string logFormat =
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        var textSink = new TextSink(logFormat);
        logger.Services.AddSingleton(textSink);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Sink(textSink)
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "bin", "log", "opl.log"),
                outputTemplate: logFormat
            )
            .CreateLogger();
        logger.AddSerilog(Log.Logger);
    }).Build();

await host.StartAsync();