using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OplWpf.Models;
using System;
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

    public AddViewModel(IOptions<Config> config, ILogger<AddViewModel> logger)
    {
        this.config = config.Value;
        this.logger = logger;
        WeakReferenceMessenger.Default.Register<RequestMessage<bool>>(this, (_, m) => m.Reply(AddApp()));
    }

    partial void OnSPortChanged(int value)
    {
        CPort = value;
    }

    private bool AddApp()
    {
        Name = Name.Trim();
        Uuid = Uuid.Trim();
        if (config.Network.Node == Uuid)
        {
            logger.LogError("自己连自己？");
            MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
            return false;
        }

        if (SPort is not > 0 and < 65536 || CPort is not > 0 and < 65536)
        {
            MessageBox.Show("端口正常范围为1-65535", "提示");
            return false;
        }

        config.AddNewApp(Name, Uuid, SPort, CPort, Type);
        logger.LogInformation("创建新的隧道{sUuid}:{sPort}--{type}>>{cPort}", Uuid, SPort, Type, CPort);
        return true;
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}