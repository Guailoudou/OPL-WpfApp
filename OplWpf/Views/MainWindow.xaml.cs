using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using OplWpf.Pages;
using OplWpf.ViewModels;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Views;

[Injection(ServiceLifetime.Singleton)]
public partial class MainWindow : Window
{
    private readonly TunnelPage _tunnelPage;
    private readonly LogPage _logPage;
    private readonly CustomizePage _customizePage;
    private readonly AboutPage _aboutPage;

    public MainWindow(MainWindowViewModel viewModel, TunnelPage tunnelPage, LogPage logPage,
        CustomizePage customizePage, AboutPage aboutPage)
    {
        InitializeComponent();
        DataContext = viewModel;
        _tunnelPage = tunnelPage;
        _logPage = logPage;
        _customizePage = customizePage;
        _aboutPage = aboutPage;
    }

    private void NavigationView_SelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        var tag = (sender.SelectedItem as NavigationViewItem)?.Tag;

        Page? page = tag switch
        {
            "Tunnel" => _tunnelPage,
            "Log" => _logPage,
            "Customize" => _customizePage,
            "About" => _aboutPage,
            _ => null
        };

        if (page != null)
        {
            Frame_Main.Navigate(page);
        }
    }
}