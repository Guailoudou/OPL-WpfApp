using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using OplWpf.ViewModels;
using System.Windows;

namespace OplWpf.Views;

/// <summary>
/// Add.xaml 的交互逻辑
/// </summary>
[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class Add : Window, IDisposable
{
    public Add(AddViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var res = WeakReferenceMessenger.Default.Send(new RequestMessage<bool>());
        if (res.Response) Close();
    }

    public void Dispose()
    {
        if (DataContext is IDisposable vm)
        {
            vm.Dispose();
        }
    }

}
