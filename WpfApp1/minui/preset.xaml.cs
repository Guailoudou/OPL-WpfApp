using System;
using System.IO;
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
using static OPL_WpfApp.MainWindow;
using userdata;
using OPL_WpfApp.cs;
//using System.Windows.Shapes;

namespace OPL_WpfApp
{
    /// <summary>
    /// preset.xaml 的交互逻辑
    /// </summary>
    public partial class preset : Window
    {
        string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "preset.json");
        Net net = new Net();
        public preset()
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
            
            if (!File.Exists(absolutePath))
            {
                _ = net.GetPreset();
            }
            else
            {
                net.getjosn();
                addp();
            }
        }
        Dictionary<string, int> Map = new Dictionary<string, int>();
        private void addp()
        {
            ComboBox box = (ComboBox)this.FindName("type");
            int index = 0;
            foreach (userdata.preset app in net.presetss.presets)
            {
                // 创建Border
                box.Items.Add(new ComboBoxItem
                {
                    Content = app.Name
                    
                });
                Map.Add(app.Name, index++);
            } 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBox box = (ComboBox)this.FindName("type");
            //TextBox SuuidText = (TextBox)this.FindName("Suuid");
            string game = box.Text;
            json json = new json();
            Logs.Out_Logs(net.presetss.presets[Map[game]].Note);
            foreach (PrTunnel key in net.presetss.presets[Map[game]].tunnel)
            {
                if(!json.newapp(Suuid.Text.Replace(" ", ""), key.Sport, key.type, key.CPort,game))return;
                
            }
            Logs.Out_Logs("已自动添加预设" + game);
            iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(net.presetss.presets[Map[game]].Note, "提示");
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
