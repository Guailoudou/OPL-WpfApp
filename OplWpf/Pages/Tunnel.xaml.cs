using OplWpf.Views;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages
{
    /// <summary>
    /// Tunnel.xaml 的交互逻辑
    /// </summary>
    public partial class Tunnel : Page
    {
        public Tunnel()
        {
            InitializeComponent();
        }

        private void ShowAddDialog(object sender, RoutedEventArgs e)
        {
            var add = new Add()
            {
                Owner = Window.GetWindow(this)
            };
            add.ShowDialog();
        }
    }
}
