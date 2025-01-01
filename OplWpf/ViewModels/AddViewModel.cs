using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OplWpf.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OplWpf.ViewModels;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public partial class AddViewModel : ObservableObject, IDisposable
{
    public string Name { get; set; } = "自定义";

    public string Uuid { get; set; } = "";

    [ObservableProperty] public partial int SPort { get; set; }

    [ObservableProperty] public partial int CPort { get; set; }

    public string Type { get; set; } = "tcp";

    private readonly Config config;
    private readonly ILogger<AddViewModel> logger;
    private readonly IMessenger messenger;

    public AddViewModel(IOptions<Config> config, ILogger<AddViewModel> logger, IMessenger messenger)
    {
        this.config = config.Value;
        this.logger = logger;
        this.messenger = messenger;
        messenger.Register<RequestMessage<bool>>(this, (_, m) => m.Reply(AddApp()));
    }

    partial void OnSPortChanged(int value)
    {
        CPort = value;
    }

    private bool AddApp()
    {
        Name = Name.Trim();
        Uuid = Uuid.Trim();
        if (string.IsNullOrEmpty(Uuid))
        {
            MessageBox.Show("UUID不能为空", "错误");
            return false;
        }

        if (config.Network.Node == Uuid)
        {
            logger.LogError("自己连自己？");
            MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
            return false;
        }

        if (SPort is not (> 0 and < 65536) || CPort is not (> 0 and < 65536))
        {
            MessageBox.Show("端口正常范围为1-65535", "提示");
            return false;
        }

        var newApp = config.AddNewApp(Name, Uuid, SPort, CPort, Type);
        messenger.Send(newApp, "add");
        return true;
    }

    public void Dispose()
    {
        messenger.UnregisterAll(this);
    }
}