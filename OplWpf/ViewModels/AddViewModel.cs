using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OplWpf.Models;

namespace OplWpf.ViewModels;

public partial class AddViewModel(string node) : ObservableValidator
{
    public string Name { get; set; } = "自定义";

    [Required(ErrorMessage = "UUID不能为空")] public string Uuid { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "端口正常范围为1-65535")]
    public int SPort
    {
        get;
        set
        {
            field = value;
            CPort = value;
        }
    }

    [Range(1, 65535, ErrorMessage = "端口正常范围为1-65535")]
    [ObservableProperty]
    public partial int CPort { get; set; }

    public string Type { get; set; } = "tcp";

    public Action? CloseWindow { get; set; }

    [RelayCommand]
    private void AddApp()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            MessageBox.Show(string.Join(
                Environment.NewLine,
                GetErrors().Select(e => e.ErrorMessage)
            ), "错误");
            return;
        }

        if (node == Uuid)
        {
            MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
            return;
        }

        CloseWindow?.Invoke();
    }
}