using System.Windows.Controls;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace OplWpf.Pages
{
    /// <summary>
    /// Log.xaml 的交互逻辑
    /// </summary>
    public partial class Log : Page
    {
        public Log()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
        }
    }
}