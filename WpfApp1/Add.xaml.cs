using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using userdata;

namespace WpfApp1
{
    /// <summary>
    /// Add.xaml 的交互逻辑
    /// </summary>
    public partial class Add : Window
    {
        public Add()
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox SuuidText = (TextBox)this.FindName("Suuid");
            TextBox SportText = (TextBox)this.FindName("Sport");
            TextBox CportText = (TextBox)this.FindName("Cport");
            ComboBox TypeText = (ComboBox)this.FindName("type");
            string Suuid = SuuidText.Text;
            string Type = TypeText.Text;
            int Sport, Cport;
            json json = new json();
            try
            {
                Sport = int.Parse(SportText.Text);
                Cport = int.Parse(CportText.Text);
                json.newapp(Suuid, Sport, Type, Cport);
            }
            catch (Exception ex)
            {
                //...
            }
            
            this.Close();
           
            
        }

        private void Sport_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox SportText = (TextBox)this.FindName("Sport");
            TextBox CportText = (TextBox)this.FindName("Cport");
            CportText.Text = SportText.Text;
        }
    }
}
