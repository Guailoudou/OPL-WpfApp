using Microsoft.Extensions.Options;
using OplWpf.Models;
using OplWpf.ViewModels;
using OplWpf.Views;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages;

/// <summary>
/// Tunnel.xaml 的交互逻辑
/// </summary>
[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class TunnelPage : Page
{
    private readonly Config config;

    public TunnelPage(TunnelViewModel viewModel, IOptions<Config> config)
    {
        InitializeComponent();
        DataContext = viewModel;
        this.config = config.Value;
    }

    private void ShowAddDialog(object sender, RoutedEventArgs e)
    {
        using var add = App.GetService<Add>();
        add.ShowDialog();
    }

    private void CheckChanged(object sender, RoutedEventArgs e)
    {
        config.Save();
    }
}
