using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using OPL_WpfApp.Utils;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using InkoreWindowHelper = iNKORE.UI.WPF.Modern.Controls.Helpers.WindowHelper;
using BackdropType = iNKORE.UI.WPF.Modern.Helpers.Styles.BackdropType;

namespace OPL_WpfApp
{
    /// <summary>
    /// 设置与主题管理（外观 / 性能 / 日志 / 通知 / 连接细节全量设置项）
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        /// <summary>初始化完成前屏蔽所有设置事件，防止回显初始值时误触发保存/弹窗</summary>
        private bool _settingsReady;

        #region 界面应用

        /// <summary>启动时应用全部界面/性能相关偏好（在性能与主题生效早期调用）</summary>
        private void ApplyUiPreferences()
        {
            settings s = set.settings;
            MotionHelper.AnimationsEnabled = s.Animations;
            double fs = s.UiFontSize >= 11 && s.UiFontSize <= 20 ? s.UiFontSize : 14;
            this.FontSize = fs;
            ApplyBackdrop(s.Backdrop);
            Logger.MaxLines = s.UiLogMaxLines;
            Logger.FileLogEnabled = s.FileLogEnabled;
            Logger.TimestampEnabled = s.TimestampInLog;
            PerformanceManager.ApplyKeepAwake(s.KeepAwake);
        }

        private void ApplyBackdrop(string backdrop)
        {
            try
            {
                BackdropType type;
                switch (backdrop)
                {
                    case "Acrylic": type = BackdropType.Acrylic; break;
                    case "None": type = BackdropType.None; break;
                    default: type = BackdropType.Mica; break;
                }
                InkoreWindowHelper.SetSystemBackdropType(this, type);
            }
            catch (Exception ex)
            {
                Logger.Log("[设置] 背景材质应用失败（系统可能不支持）：" + ex.Message, "提示");
            }
        }

        private void ApplyCompactNav(bool compact)
        {
            if (MainTabs == null) return;
            foreach (object obj in MainTabs.Items)
            {
                if (!(obj is TabItem tab) || !(tab.Header is StackPanel sp)) continue;
                sp.Width = compact ? 56 : 174;
                sp.HorizontalAlignment = compact ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                foreach (UIElement child in sp.Children)
                {
                    if (child is TextBlock tb)
                        tb.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
                    else if (child is FontIcon icon)
                        icon.Margin = compact ? new Thickness(0) : new Thickness(0, 0, 10, 0);
                }
            }
        }

        /// <summary>页面切换过场（仅整页单次淡入上移，卡片不再错峰级联）</summary>
        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 只响应主标签页自身的切换，屏蔽页内 ComboBox/ListBox 冒泡上来的 SelectionChanged
            if (!ReferenceEquals(e.OriginalSource, MainTabs)) return;
            if (!(MainTabs.SelectedItem is TabItem item)) return;
            if (item.Content is FrameworkElement page)
                MotionHelper.PlayPageTransition(page);
        }

        #endregion

        #region 主题与颜色

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
            var Border = sender as System.Windows.Controls.Border;
            SolidColorBrush color = Border.Background as SolidColorBrush;
            ThemeManager.Current.AccentColor = color.Color;
            ColorBlock.SelectColor = color;
            set.settings.Color = color.Color.ToString();
            set.Write();
            MessageBox.Show($"设置颜色成功{color.Color} 部分样式可能需要重启生效");
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

        #endregion

        #region 开关类设置

        /// <summary>带二次确认的“关闭”开关模板：取消则弹回开启</summary>
        private bool ConfirmTurnOff(string message, iNKORE.UI.WPF.Modern.Controls.ToggleSwitch sender)
        {
            MessageBoxResult result = MessageBox.Show(
                message, "警告", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.OK)
            {
                _settingsReady = false;
                sender.IsOn = true;
                _settingsReady = true;
                return false;
            }
            return true;
        }

        private void Autoup_opn_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            var sw = (iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender;
            if (sw.IsOn)
            {
                set.settings.Auto_upop = true;
                set.Write();
            }
            else if (ConfirmTurnOff(
                "你确定要关闭 openp2p 文件校验吗，你需要知道你在做什么，如果你不了解这个的作用请不用动他！！!", sw))
            {
                set.settings.Auto_upop = false;
                set.Write();
            }
        }

        private void Autoup_etn_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            var sw = (iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender;
            if (sw.IsOn)
            {
                set.settings.Auto_upet = true;
                set.Write();
            }
            else if (ConfirmTurnOff(
                "确定要关闭 EasyTier 核心自动更新吗？旧版本可能无法使用新节点或新协议。", sw))
            {
                set.settings.Auto_upet = false;
                set.Write();
            }
        }

        private void Autoupn_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            var sw = (iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender;
            if (sw.IsOn)
            {
                set.settings.Auto_up = true;
                set.Write();
            }
            else if (ConfirmTurnOff(
                "你确定要关闭自动升级吗，你需要知道你在做什么，这可能会导致你的程序存在 bug！！!", sw))
            {
                set.settings.Auto_up = false;
                set.Write();
            }
        }

        private void Autoup_bootn_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            if (((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn)
            {
                MessageBox.Show("开启此功能后开机仍然可能会弹出以管理员启动的同意弹窗\r想要不提示，可以在电脑底部搜索框输入 UAC，打开\u201c更改用户账户控制设置\u201d把权限降到最低即可", "提示");
                AutoStartWith.AddToStartup();
            }
            else AutoStartWith.RemoveFromStartup();
        }

        private void Autoup_openn_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.Auto_open = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void MinimizeOnStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.MinimizeOnStartup = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void minimize_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.minimize = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void Ispwarning_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.ispwarning = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void CleanLog_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.CleanLogOnExit = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void NoticePopup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.NoticePopup = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void TrayBalloon_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.TrayBalloonTip = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void ToastTunnel_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.ToastOnTunnelConnected = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void AskOnClose_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.qusminimize = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void FileLog_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.FileLogEnabled = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            Logger.FileLogEnabled = set.settings.FileLogEnabled;
            set.Write();
        }

        private void Timestamp_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.TimestampInLog = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            Logger.TimestampEnabled = set.settings.TimestampInLog;
            set.Write();
        }

        private void BackgroundLog_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.AutoHideLogOnBackground = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void TrimMinimize_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.TrimOnMinimize = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void AggressiveTrim_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.AggressiveTrim = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void KeepAwake_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.KeepAwake = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            PerformanceManager.ApplyKeepAwake(set.settings.KeepAwake);
            set.Write();
        }

        private void Anim_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.Animations = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            MotionHelper.AnimationsEnabled = set.settings.Animations;
            set.Write();
        }

        private void CompactNav_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.CompactNav = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            ApplyCompactNav(set.settings.CompactNav);
            set.Write();
        }

        private void AutoCopyCode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.AutoCopyLinkCode = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void ConfirmCloseAll_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.ConfirmBeforeCloseAll = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void HumanTraffic_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.HumanReadableTraffic = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        private void Beta_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.beta = ((iNKORE.UI.WPF.Modern.Controls.ToggleSwitch)sender).IsOn;
            set.Write();
        }

        #endregion

        #region 下拉/文本类设置

        private void UiFontCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            double[] sizes = { 12, 13, 14, 16, 18 };
            int i = UiFontCombo.SelectedIndex;
            if (i < 0 || i >= sizes.Length) return;
            set.settings.UiFontSize = sizes[i];
            this.FontSize = sizes[i];
            set.Write();
        }

        private void BackdropCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            string[] values = { "Mica", "Acrylic", "None" };
            int i = BackdropCombo.SelectedIndex;
            if (i < 0 || i >= values.Length) return;
            set.settings.Backdrop = values[i];
            ApplyBackdrop(values[i]);
            set.Write();
        }

        private void PerfModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            string[] values = { "HighPerformance", "Balanced", "PowerSave" };
            int i = PerfModeCombo.SelectedIndex;
            if (i < 0 || i >= values.Length) return;
            set.settings.PerfMode = values[i];
            set.Write();
        }

        private void EtPollCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            int[] values = { 2, 5, 10, 30 };
            int i = EtPollCombo.SelectedIndex;
            if (i < 0 || i >= values.Length) return;
            set.settings.EtPollSeconds = values[i];
            set.Write();
        }

        private void TrafficPollCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            int[] values = { 1, 2, 5 };
            int i = TrafficPollCombo.SelectedIndex;
            if (i < 0 || i >= values.Length) return;
            set.settings.TrafficPollSeconds = values[i];
            set.Write();
        }

        private void LogLinesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            int[] values = { 100, 300, 500, 1000, 2000 };
            int i = LogLinesCombo.SelectedIndex;
            if (i < 0 || i >= values.Length) return;
            set.settings.UiLogMaxLines = values[i];
            Logger.MaxLines = values[i];
            set.Write();
        }

        private void DefaultProtocolCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_settingsReady) return;
            set.settings.DefaultProtocol = DefaultProtocolCombo.SelectedIndex == 1 ? "udp" : "tcp";
            set.Write();
        }

        private void EtArgsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string value = (EtArgsBox.Text ?? "").Trim();
            // 基础防线：禁止破坏引号配对的输入
            if (value.Count(c => c == '"') % 2 != 0)
            {
                MessageBox.Show("参数中的引号不成对，已忽略本次修改。", "提示");
                EtArgsBox.Text = set.settings.EasyTierExtraArgs ?? "";
                return;
            }
            if (value == (set.settings.EasyTierExtraArgs ?? "")) return;
            set.settings.EasyTierExtraArgs = value;
            set.Write();
        }

        #endregion

        #region 初始化回显

        private static int IndexOfValue<T>(T[] values, T value)
        {
            int i = Array.IndexOf(values, value);
            return i < 0 ? 1 : i;
        }

        private void Initialization(bool temp = false)
        {
            settings s = set.settings;

            // —— 先回显全部初始状态（_settingsReady=false 时事件不生效）——
            Autoup_opn.IsOn = s.Auto_upop;
            Autoup_etn.IsOn = s.Auto_upet;
            Autoupn.IsOn = s.Auto_up;
            Autoup_openn.IsOn = s.Auto_open;
            MinimizeOnStartupToggle.IsOn = s.MinimizeOnStartup;
            minimize.IsOn = s.minimize;
            Ispwarning.IsOn = s.ispwarning;
            CleanLogCheck.IsOn = s.CleanLogOnExit;
            NoticePopupToggle.IsOn = s.NoticePopup;
            TrayBalloonToggle.IsOn = s.TrayBalloonTip;
            ToastTunnelToggle.IsOn = s.ToastOnTunnelConnected;
            AskOnCloseToggle.IsOn = s.qusminimize;
            FileLogToggle.IsOn = s.FileLogEnabled;
            TimestampToggle.IsOn = s.TimestampInLog;
            BackgroundLogToggle.IsOn = s.AutoHideLogOnBackground;
            TrimMinimizeToggle.IsOn = s.TrimOnMinimize;
            AggressiveTrimToggle.IsOn = s.AggressiveTrim;
            KeepAwakeToggle.IsOn = s.KeepAwake;
            AnimToggle.IsOn = s.Animations;
            CompactNavToggle.IsOn = s.CompactNav;
            AutoCopyCodeToggle.IsOn = s.AutoCopyLinkCode;
            ConfirmCloseAllToggle.IsOn = s.ConfirmBeforeCloseAll;
            HumanTrafficToggle.IsOn = s.HumanReadableTraffic;
            BetaToggle.IsOn = s.beta;

            UiFontCombo.SelectedIndex = IndexOfValue(new double[] { 12, 13, 14, 16, 18 }, s.UiFontSize);
            BackdropCombo.SelectedIndex = s.Backdrop == "Acrylic" ? 1 : s.Backdrop == "None" ? 2 : 0;
            PerfModeCombo.SelectedIndex = s.PerfMode == "HighPerformance" ? 0 : s.PerfMode == "PowerSave" ? 2 : 1;
            EtPollCombo.SelectedIndex = IndexOfValue(new[] { 2, 5, 10, 30 }, s.EtPollSeconds);
            TrafficPollCombo.SelectedIndex = IndexOfValue(new[] { 1, 2, 5 }, s.TrafficPollSeconds);
            LogLinesCombo.SelectedIndex = IndexOfValue(new[] { 100, 300, 500, 1000, 2000 }, s.UiLogMaxLines);
            DefaultProtocolCombo.SelectedIndex = s.DefaultProtocol == "udp" ? 1 : 0;
            EtArgsBox.Text = s.EasyTierExtraArgs ?? "";

            ApplyCompactNav(s.CompactNav);

            try
            {
                if (AutoStartWith.IsInStartup())
                {
                    Autoup_bootn.IsOn = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{ex.Message}", "错误");
            }

            if (sjson.config.LogLevel == 2)
            {
                sjson.config.LogLevel = 1;
                sjson.Save();
            }

            // —— 回显结束，放行全部事件 ——
            _settingsReady = true;

            // —— 设备指纹校验（原有逻辑保持不变）——
            string newuuid;
            try
            {
                newuuid = GetSmBIOSUUID();
                if (newuuid == null) throw new ArgumentException("未获取到正确数据");
            }
            catch (Exception e)
            {
                Logger.Log("获取设备 id 失败 : " + e.Message, "错误");
                return;
            }
            Logger.Log("设备 ID 为：" + newuuid, "信息");
            if (set.settings.csproduct == null || set.settings.csproduct == "")
                set.settings.csproduct = newuuid;
            else if (set.settings.csproduct != newuuid)
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

        #endregion
    }
}
