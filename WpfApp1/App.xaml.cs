using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 应用程序启动时的自定义逻辑
            //var mainWindow = new MainWindow();
            //mainWindow.Show();
            //var Add = new Add();
            //Add.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 应用程序退出时的清理操作
            base.OnExit(e);
        }
    }
}
