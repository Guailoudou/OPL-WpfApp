using OplWpf.ViewModels;
using OplWpf.Views;

namespace OplWpf.Services;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public class DialogService
{
    public bool? Show(AddViewModel addViewModel)
    {
        var addView = new Add(addViewModel);
        return addView.ShowDialog();
    }
}
