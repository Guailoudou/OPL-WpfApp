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

namespace WpfApp1
{
    /// <summary>
    /// edit.xaml 的交互逻辑
    /// </summary>
    public partial class edit : Window
    {
        private int index;
        public edit(int index)
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
            this.index = index;
            userdata.json json = new userdata.json();
            TextBox SuuidText = (TextBox)this.FindName("Suuid");
            TextBox SportText = (TextBox)this.FindName("Sport");
            TextBox CportText = (TextBox)this.FindName("Cport");
            ComboBox TypeText = (ComboBox)this.FindName("type");
            SportText.Text = json.config.Apps[index].DstPort.ToString();
            CportText.Text = json.config.Apps[index].SrcPort.ToString();
            SuuidText.Text = json.config.Apps[index].PeerNode;
            TypeText.Text = json.config.Apps[index].Protocol;
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
            userdata.json json = new userdata.json();
            try
            {
                Sport = int.Parse(SportText.Text);
                Cport = int.Parse(CportText.Text);
                json.del(index);
                json.newapp(Suuid, Sport, Type, Cport);
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误，异常的输入:" + ex, "警告");

            }
            this.Close();
        }
    }
}
