using iNKORE.UI.WPF.Modern.Controls;
using OplWpf.Pages;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;
using OplWpf.ViewModels;
using OplWpf.Models;

namespace OplWpf.Views;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public partial class MainWindow : Window
{
    private readonly TunnelPage tunnelPage;
    public readonly LogPage logPage;
    public readonly AboutPage aboutPage;

    public MainWindow(MainWindowViewModel viewModel, TunnelPage tunnelPage, LogPage logPage, AboutPage aboutPage, Openp2p openp2p)
    {
        InitializeComponent();
        DataContext = viewModel;
        this.tunnelPage = tunnelPage;
        this.logPage = logPage;
        this.aboutPage = aboutPage;
        NavigationView_Root.SelectedItem = Navigation_Tunnel;
        Closing += (_, _) => openp2p.Stop();
    }

    private void NavigationView_SelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        var item = sender.SelectedItem;
        Page? page = null;

        if (Equals(item, Navigation_Tunnel))
        {
            page = tunnelPage;
        }
        else if (Equals(item, Navigation_Log))
        {
            page = logPage;
        }
        //else if (item == NavigationViewItem_Apps)
        //{
        //    page = Page_Apps;
        //}
        else if (Equals(item, Navigation_About))
        {
            page = aboutPage;
        }

        if (page != null)
        {
            Frame_Main.Navigate(page);
        }
    }
}