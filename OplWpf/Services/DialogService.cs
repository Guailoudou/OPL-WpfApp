using OplWpf.Models;
using OplWpf.ViewModels;
using OplWpf.Views;

namespace OplWpf.Services;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public class DialogService(ConfigManager configManager)
{
    public AppConfig? Show()
    {
        var vm = new AddViewModel(configManager.Config.Network.Node);
        var addView = new Add(vm);
        return addView.ShowDialog() == true ? new AppConfig(vm.Name, vm.Type, vm.CPort, vm.Uuid, vm.SPort) : null;
    }
}