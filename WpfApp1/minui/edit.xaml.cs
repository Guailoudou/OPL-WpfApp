﻿using Newtonsoft.Json;
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
            TextBox names = (TextBox)this.FindName("names");
            ComboBox TypeText = (ComboBox)this.FindName("type");
            SportText.Text = json.config.Apps[index].DstPort.ToString();
            CportText.Text = json.config.Apps[index].SrcPort.ToString();
            names.Text = json.config.Apps[index].AppName;
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
            TextBox names = (TextBox)this.FindName("names");
            ComboBox TypeText = (ComboBox)this.FindName("type");
            json json = new json();
            json.config.Apps[index].PeerNode = SuuidText.Text.Replace(" ", "");
            json.config.Apps[index].Protocol = TypeText.Text.Replace(" ", "");
            json.config.Apps[index].AppName = names.Text.Replace(" ", "");
            try
            {
                int Sport = int.Parse(SportText.Text.Replace(" ", ""));
                int Cport = int.Parse(CportText.Text.Replace(" ", ""));
                if (Sport > 0 && Sport <= 65535 && Cport > 0 && Cport <= 65535)
                {
                    json.config.Apps[index].DstPort = int.Parse(SportText.Text.Replace(" ", ""));
                    json.config.Apps[index].SrcPort = int.Parse(CportText.Text.Replace(" ", ""));
                }
                else
                {
                    iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("存在错误数据 端口正常范围为1-65535", "提示");
                    return;
                }

            }
            catch (Exception ex)
            {
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("错误，异常的输入:" + ex, "警告");
                return;
            }
            string ujson = JsonConvert.SerializeObject(json.config, Formatting.Indented);
            json.wejson(ujson);
            this.Close();
        }
    }
}
