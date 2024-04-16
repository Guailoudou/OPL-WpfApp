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
            //userdata.UserData userData = new userdata.UserData();
            //userdata.json sjson = new userdata.json();
            this.DataContext = userData;
           
            sjson.newjson(userData);
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
    }
}
