using OplWpf.ViewModels;
using System.Windows;

namespace OplWpf.Views;

public partial class Add : Window
{
    public Add(AddViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseWindow = () => DialogResult = true;
    }
}