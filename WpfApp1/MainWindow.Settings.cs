using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using OPL_WpfApp.Utils;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// 设置与主题管理
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OperatingSystem os = Environment.OSVersion;
            Version vers = os.Version;
            
            if (vers.Major <= 6 && vers.Minor <= 1)
            {
                MessageBox.Show("当前系统过低，主题设置可能存在意想不到的后果", "警告");
            }
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "跟随系统":
                        ThemeManager.Current.ApplicationTheme = null;
                        SetThene("");
                        break;
                    case "浅色":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        SetThene("Light");
                        break;
                    case "深色":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        SetThene("Dark");
                        break;
                }
            }
        }
        
        private void ColorPicker_Set(object sender, RoutedEventArgs e)
        {
            SolidColorBrush color = ColorBlock.SelectColor;
            ThemeManager.Current.AccentColor = color.Color;
            set.settings.Color = color.Color.ToString();
            set.Write();
            MessageBox.Show($"设置颜色成功{color.Color} 部分样式可能需要重启生效");
        }

        private void ColorPicker_ReSet(object sender, RoutedEventArgs e)
        {
            set.settings.Color = "";
            set.Write();
            MessageBox.Show($"已重置，重启生效");
        }

        private void SetReColor(object sender, RoutedEventArgs e)
        {
            var Border = sender as Border;
            SolidColorBrush color = Border.Background as SolidColorBrush;
            ThemeManager.Current.AccentColor = color.Color;
            ColorBlock.SelectColor = color;
            set.settings.Color = color.Color.ToString();
            set.Write();
            MessageBox.Show($"设置颜色成功{color.Color} 部分样式可能需要重启生效");
        }

        private void Autoup_op(object sender, RoutedEventArgs e)
        {
            set.settings.Auto_upop = true;
            set.Write();
        }

        private void UnAutoup_op(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "你确定要关闭 openp2p 文件校验吗，你需要知道你在做什么，如果你不了解这个的作用请不用动他！！!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question); 
            if (result == MessageBoxResult.OK)
            {
                set.settings.Auto_upop = false;
                set.Write();
            }
            else
            {
                Autoup_opn.IsChecked = true;
                return;
            }
        }

        private void Autoup(object sender, RoutedEventArgs e)
        {
            set.settings.Auto_up = true;
            set.Write();
        }

        private void UnAutoup(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "你确定要关闭自动升级吗，你需要知道你在做什么，这可能会导致你的程序存在 bug！！!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                set.settings.Auto_up = false;
                set.Write();
            }
            else
            {
                Autoupn.IsChecked = true;
                return;
            }
        }

        private void Auto_boot(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("开启此功能后开机仍然可能会弹出以管理员启动的同意弹窗\r想要不提示，可以在电脑底部搜索框输入 UAC，打开\u201c更改用户账户控制设置\u201d把权限降到最低即可", "提示");
            AutoStartWith.AddToStartup();
        }

        private void Auto_open(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_open = true;
            set.Write();
        }

        private void UnAuto_boot(object sender, RoutedEventArgs e)
        {
            AutoStartWith.RemoveFromStartup();
        }

        private void UnAuto_open(object sender, RoutedEventArgs e)
        {
            set set = new set();
            set.settings.Auto_open = false;
            set.Write();
        }

        private void Ispwarn(object sender, RoutedEventArgs e)
        {
            set.settings.ispwarning = true;
            set.Write();
        }

        private void UnIspwarn(object sender, RoutedEventArgs e)
        {
            set.settings.ispwarning = false;
            set.Write();
        }

        private void Minimize_ev(object sender, RoutedEventArgs e)
        {
            set.settings.minimize = true;
            set.Write();
        }

        private void Unminimize_ev(object sender, RoutedEventArgs e)
        {
            set.settings.minimize = false;
            set.Write();
        }

        private void SetThene(string theme)
        {
            set.settings.Theme = theme;
            set.Write();
        }

        private void GetTheme()
        {
            string Theme = set.settings.Theme;
            if (Theme != null)
                switch (Theme)
                {
                    case "":
                        ThemeManager.Current.ApplicationTheme = null;
                        Theme_auto.IsSelected = true;
                        break;
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        Theme_Light.IsSelected = true;
                        break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        Theme_Dark.IsSelected = true;
                        break;
                }
            else
            {
                ThemeManager.Current.ApplicationTheme = null;
                Theme_auto.IsSelected = true;
            }
        }

        private void Initialization(bool temp =false)
        {
            if (set.settings.Auto_upop)
            {
                Autoup_opn.IsChecked = true;
            }
            if(set.settings.Auto_up)
            {
                Autoupn.IsChecked = true;
            }
            if (set.settings.Auto_open)
            {
                Autoup_openn.IsChecked = true;
                if(!temp)Strapp();
            }
            try
            {
                if (AutoStartWith.IsInStartup())
                {
                    Autoup_bootn.IsChecked = true;
                }
            }catch (Exception ex)
            {
                Logger.Log($"{ex.Message}","错误");
            }
            
            if (set.settings.ispwarning)
            {
                Ispwarning.IsChecked = true;
            }
            if (set.settings.minimize)
            {
                minimize.IsChecked = true;
            }
            if (sjson.config.LogLevel == 2)
            {
                sjson.config.LogLevel = 1;
                sjson.Save();
            }
            Autoup_bootn.Checked += Auto_boot;
            Autoup_bootn.Unchecked += UnAuto_boot;

            Autoup_opn.Checked += Autoup_op;
            Autoup_opn.Unchecked += UnAutoup_op;

            Autoupn.Checked += Autoup;
            Autoupn.Unchecked += UnAutoup;

            Autoup_openn.Checked += Auto_open;
            Autoup_openn.Unchecked += UnAuto_open;

            Ispwarning.Checked += Ispwarn;
            Ispwarning.Unchecked += UnIspwarn;

            minimize.Checked += Minimize_ev;
            minimize.Unchecked += Unminimize_ev;

            string newuuid;
            try
            {
                newuuid = GetSmBIOSUUID();
                if (newuuid == null) throw new ArgumentException("未获取到正确数据");
            }
            catch (Exception e)
            {
                Logger.Log("获取设备 id 失败 : "+e.Message,"错误");
                return;
            }
            Logger.Log("设备 ID 为："+newuuid,"信息");
            if(set.settings.csproduct==null||set.settings.csproduct=="")
                set.settings.csproduct = newuuid;
            else if(set.settings.csproduct!=newuuid)
            {
                MessageBoxResult result = MessageBox.Show(
                "检测到设备更改，是否要重置 UID？（如果这是别人发你的，请点确定，否则可能会导致运行问题）",
                "提示",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    userData.ResetUID();
                    TextBox UIDTextBox = (TextBox)this.FindName("UID");
                    UIDTextBox.Text = userData.UID;
                    sjson.newjson(userData);
                    MessageBox.Show("已重置 UID，新的 UID 为：" + userData.UID, "提示");
                    Relist();
                }
                set.settings.csproduct = newuuid;
            }
            set.Write();
            tunnel.csh(tunspeed);
        }
    }
}
