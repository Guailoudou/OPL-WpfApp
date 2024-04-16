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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        userdata.UserData userData = new userdata.UserData();
        userdata.json sjson = new userdata.json();
        public MainWindow()
        {
            InitializeComponent();
            // 设置窗口位置居中显示
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
            //userdata.UserData userData = new userdata.UserData();
            //userdata.json sjson = new userdata.json();
            this.DataContext = userData;
           
            //sjson.newjson(userData);
            //wpfWebBrowser.Navigate("https://blog.gldhn.top/2024/04/15/opl_help/");
        }
        //userdata.UserData userData = new userdata.UserData();
        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //MessageBox.Show("你左键点击了我", "提示");
        //}

        private void CopyUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox uuidTextBox = (TextBox)this.FindName("UUID");
            Clipboard.SetText(uuidTextBox.Text);
            MessageBox.Show("复制成功", "提示");
        }

        private void ResetUUID_Button_Click(object sender, RoutedEventArgs e)
        {
            // 显示确认对话框
            MessageBoxResult result = MessageBox.Show(
                "你确定要重置吗?会导致失去所有已有隧道配置!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                userData.ResetUUID();
                TextBox uuidTextBox = (TextBox)this.FindName("UUID");
                uuidTextBox.Text = userData.UUID;
                sjson.newjson(userData);
                MessageBox.Show("已重置UUID,新的UUID为：" + userData.UUID, "提示");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Form1_Load(object sender, RoutedEventArgs e)
        {
           // wpfWebBrowser.Navigate("https://blog.gldhn.top/2024/04/15/opl_help/");
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Add Add = new Add();
            Add.Owner = this;
            Add.Topmost = true;
            Add.ShowDialog();
        }
        public void relist()
        {
            // 获取ListBox控件
            ListBox listBox = this.FindName("sdlist") as ListBox;

            // 创建Border
            Border border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            // 创建内部Grid
            Grid grid = new Grid
            {
                Height = 63,
                Width = 700
            };

            // 添加各个子控件到Grid
            grid.Children.Add(new Label
            {
                Content = "隧道1",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 3, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            });

            grid.Children.Add(new Label
            {
                Content = "目标UUID：erererererereer",
                Margin = new Thickness(10, 29, 538, 10)
            });

            grid.Children.Add(new Label
            {
                Content = "远程端口：25555",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(177, 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            });

            grid.Children.Add(new Label
            {
                Content = "本地端口：25566",
                Margin = new Thickness(177, 32, 371, 7)
            });

            CheckBox checkBox = new CheckBox
            {
                Content = "启用",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(630, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            checkBox.Checked += CheckBox_Checked; // 需要定义CheckBox_Checked事件处理程序
            grid.Children.Add(checkBox);

            Button closeButton = new Button
            {
                Content = " X ",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(678, 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            grid.Children.Add(closeButton);

            Button editButton = new Button
            {
                Content = "编辑",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(576, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(editButton);

            // 将Grid添加到Border
            border.Child = grid;

            // 将Border包装在ListBoxItem中并添加到ListBox
            ListBoxItem item = new ListBoxItem { Content = border };
            listBox.Items.Add(item);
        }
    }
}
