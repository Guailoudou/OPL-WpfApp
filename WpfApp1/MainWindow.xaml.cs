using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Windows;
using userdata;
using OPL_WpfApp.easyTier;
using OPL_WpfApp.Utils;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using MenuItem = System.Windows.Forms.MenuItem;
using MouseButtons = System.Windows.Forms.MouseButtons;
using ContextMenu = System.Windows.Forms.ContextMenu;

namespace OPL_WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        public UserData userData;
        json sjson;
        tunnel tunnel = new tunnel();
        private etstart ets;
        set set = new set();
        Net net = new Net();

        private bool _on;
        private bool _eton;
        public bool on
        {
            get => _on;
            set { _on = value; UpdateActionAvailability(); }
        }
        public bool eton
        {
            get => _eton;
            set { _eton = value; UpdateActionAvailability(); }
        }
        public static bool over = true;
        int tcpnum = 0;
        string opname = "openp2p.exe";
        NotifyIcon notifyIcon;

        public MainWindow_opl(string[] args)
        {
            InitializeComponent();
            Logger logger = new Logger(richOutput); // 最先初始化日志，保证后续任何异常都能被真实记录
            // —— 性能与动效基础设施（需在首次日志/动画前生效）——
            ApplyUiPreferences();
            PerformanceManager.Attach(this);
            MotionHelper.AttachHalo(fstertHaloHost, fstert);
            MotionHelper.AttachHalo(tunHaloHost, tunellipse);
            if (set.settings.EasyTierPeers != null)
                foreach (string peer in set.settings.EasyTierPeers)
                    EasyTierPeersList.Items.Add(peer);
            WindowHelper.CenterOnScreen(this);
            Uplog uplog = new Uplog(uplogbox);
            userData = new userdata.UserData();
            sjson = new userdata.json();
            if (sjson.config == null || sjson.config.Network == null)
            {
                Logger.Log("[错误]配置初始化失败，主窗口无法继续加载", "错误");
                return;
            }

            this.DataContext = userData;
            OperatingSystem os = Environment.OSVersion;
            Version vers = os.Version;
            Logger.Log($"[信息] 程序启动，当前版本：{Getversion()}，更新包号：{Net.Getpvn()}，系统版本：{vers}");
            if (vers.Major <= 6 && vers.Minor <= 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                Logger.Log("[提示] 当前操作系统版本过低，为防止显示问题已自动切换为黑夜模式");
            }
            else GetTheme();
            
            _ = net.GetPreset(ServersCombo);
            _ = net.Getthank(thank);
            _ = CheckAndShowNewNotices();
            Relist();
            UID.Text = sjson.config.Network.Node;
            share.Text = sjson.config.Network.ShareBandwidth.ToString();
            ver.Content = Getversion() + " - "+ Net.Getpvn();
            string bgColor = ExtractBackgroundColor(args);
            if(bgColor != null) ColorBlock.SelectColor = new System.Windows.Media.SolidColorBrush(set.ParseColor(bgColor));
            
            Initialization();

            ets = new etstart(this);
            UpdateActionAvailability();

            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Text = "OPL 联机工具";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            
            MenuItem show = new MenuItem("显示窗口");
            show.Click += Show;
            MenuItem exit = new MenuItem("退出");
            exit.Click += Close;
            MenuItem[] mis = new MenuItem[] { show, exit };
            notifyIcon.ContextMenu = new ContextMenu(mis);

            this.notifyIcon.MouseDoubleClick += (o, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Show(o, e);
                }
            };
        }

        private void UpdateActionAvailability()
        {
            if (!IsInitialized) return;
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(UpdateActionAvailability));
                return;
            }

            bool canEdit = !on;
            QuickAddButton.IsEnabled = canEdit;
            NewConnectionButton.IsEnabled = canEdit;
            CloseAllButton.IsEnabled = canEdit;
            ExportLogButton.IsEnabled = canEdit;
            ResetProgramButton.IsEnabled = canEdit;
            CreateEasyTierButton.IsEnabled = canEdit;
            JoinEasyTierButton.IsEnabled = canEdit;
            LeaveEasyTierButton.IsEnabled = eton;
            openbutton.IsEnabled = !eton;

            if (!tunnel.getruning())
            {
                tunbutton.IsEnabled = canEdit;
                tunjoinbutton.IsEnabled = canEdit;
            }

            foreach (Controls.TunnelListItem item in sdlist.Items)
                item.SetEditingEnabled(canEdit);
        }
    }
}
