﻿using iNKORE.UI.WPF.Modern;
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

        public bool on = false;
        public bool eton = false;
        public static bool over = true;
        int tcpnum = 0;
        string opname = "openp2p.exe";
        NotifyIcon notifyIcon;

        public MainWindow_opl(string[] args)
        {
            InitializeComponent();
            WindowHelper.CenterOnScreen(this);
            Logger logger = new Logger(richOutput);
            Uplog uplog = new Uplog(uplogbox);
            this.DataContext = userData;
            userData = new userdata.UserData();
            sjson = new userdata.json();
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
            _ = GetsayText();
            Relist();
            UID.Text = sjson.config.Network.Node;
            share.Text = sjson.config.Network.ShareBandwidth.ToString();
            ver.Content = Getversion() + " - "+ Net.Getpvn();
            string bgColor = ExtractBackgroundColor(args);
            if(bgColor != null) ColorBlock.SelectColor = new System.Windows.Media.SolidColorBrush(set.ParseColor(bgColor));
            
            Initialization();

            ets = new etstart(this);

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
    }
}
