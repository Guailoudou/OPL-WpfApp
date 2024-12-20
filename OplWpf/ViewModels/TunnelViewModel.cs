using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using System.Windows;
using OplWpf.Models;

namespace OplWpf.ViewModels;

public partial class TunnelViewModel
{
    public Config Config => ConfigManager.Instance.Config;

    [RelayCommand]
    public void CopyUid(string uid)
    {
        Clipboard.SetText(uid);
        MessageBox.Show("复制成功", "提示");
    }
}
