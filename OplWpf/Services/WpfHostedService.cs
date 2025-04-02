using System.Windows;
using Microsoft.Extensions.Hosting;

namespace OplWpf.Services
{
    internal class WpfHostedService<TApplication, TWindow>(
        TApplication application,
        TWindow window,
        IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
        where TApplication : Application
        where TWindow : Window
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            application.Run(window);
            hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}