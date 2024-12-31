using OplWpf.ViewModels;
using System.Windows.Controls;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages;

/// <summary>
/// Log.xaml 的交互逻辑
/// </summary>
[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class LogPage : Page
{
    public LogPage(LogViewModel logViewModel)
    {
        InitializeComponent();
        DataContext = logViewModel;
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        LogTextBox.CaretIndex = LogTextBox.Text.Length;
    }
}