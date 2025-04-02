using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace OplWpf;

[Injection(ServiceLifetime.Singleton)]
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }
}