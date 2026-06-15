using System;
using System.Windows;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls;
using userdata;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// 窗口生命周期与托盘图标管理
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        /// <summary>
        /// 显示窗体
        /// </summary>
        private void Show(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 窗体状态改变
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            // 最小化处理逻辑
        }

        // 监听点击关闭按钮
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (set.settings.qusminimize)
            {
                var result = MessageBox.Show("是否要隐藏？\n点否直接退出\n下次不会再提醒，后续可前往设置更改", "提示", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    set.settings.minimize = true;
                }
                else
                {
                    set.settings.minimize = false;
                }
                set.settings.qusminimize = false;
                set.Write();
            }
            
            if (set.settings.minimize)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
                this.notifyIcon.Visible = true;
                return;
            }
            
            notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Kill();
            }
            Stop();
            if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
            if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
            if(tunbutton.Content.ToString() != "创建/开启网络" || tunjoinbutton.Content.ToString() != "连接网络")
            {
                tunnel.OpenTunnel(tunjoinbutton, 1, 1);
            }

            ets?.Stop();
            base.OnClosed(e);
        }

        private static string ExtractBackgroundColor(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-bg=", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = arg.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        return parts[1];  
                    }
                }
            }
            return null; 
        }
    }
}
