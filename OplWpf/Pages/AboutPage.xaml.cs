using OplWpf.ViewModels;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class AboutPage : Page
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}