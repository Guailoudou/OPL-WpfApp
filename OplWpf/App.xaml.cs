using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OplWpf.Models;
using OplWpf.Services;
using OplWpf.Views;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace OplWpf;

public partial class App : Application
{
    private readonly static IHost host = CreateHostBuilder().Build();

    [STAThread]
    public static void Main()
    {
        host.Start();

        var app = new App();
        app.InitializeComponent();
        app.MainWindow = GetService<MainWindow>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static T GetService<T>() where T : notnull
    {
        return host.Services.GetRequiredService<T>();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
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

                services.AddSingleton<HeartBeat>();

                services.AddHostedService<HeartBeatService>();
                services.AddHostedService<UpdateService>();
            })
            .ConfigureLogging((context, logger) =>
            {
                logger.ClearProviders();
                const string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
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
            });
    }
}
