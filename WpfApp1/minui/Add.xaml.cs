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

namespace OPL_WpfApp
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
            TextBox Names = (TextBox)this.FindName("names");
            ComboBox TypeText = (ComboBox)this.FindName("type");
            string Suuid = SuuidText.Text.Replace(" ", "");
            string Type = TypeText.Text;
            string names = Names.Text.Replace(" ", "");
            int Sport, Cport;
            json json = new json();
            try
            {
                Sport = int.Parse(SportText.Text.Replace(" ", ""));
                Cport = int.Parse(CportText.Text.Replace(" ", ""));
                if (Suuid != "" && Type != "" && Sport > 0 && Sport <= 65535)
                    if (!json.newapp(Suuid, Sport, Type, Cport, names)) return; 
                    else 
                    {
                        this.Close();
                    }
                else
                {
                    iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("存在未填数据或错误数据 端口正常范围为1-65535", "提示");
                    return;
                }
            }
            catch (Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("错误，异常的输入:"+ex, "警告");
                return;
                
            }
            
            
           
            
        }

        private void Sport_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox SportText = (TextBox)this.FindName("Sport");
            TextBox CportText = (TextBox)this.FindName("Cport");
            CportText.Text = SportText.Text;
        }

        private void Preset(object sender, RoutedEventArgs e)
        {
            preset ed = new preset();
            ed.Owner = this;
            ed.Topmost = true;
            ed.ShowDialog();
            this.Close();
        }
    }
}
