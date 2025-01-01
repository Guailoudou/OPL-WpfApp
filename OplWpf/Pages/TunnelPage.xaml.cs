using OplWpf.ViewModels;
using OplWpf.Views;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class TunnelPage : Page
{
    public TunnelPage(TunnelViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ShowAddDialog(object sender, RoutedEventArgs e)
    {
        using var add = App.GetService<Add>();
        add.ShowDialog();
    }
}
