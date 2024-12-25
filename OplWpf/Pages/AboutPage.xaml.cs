using OplWpf.ViewModels;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            DataContext = new AboutViewModel();
        }
    }
}