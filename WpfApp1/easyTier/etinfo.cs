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
using System.Windows.Threading;
using static OPL_WpfApp.MainWindow_opl;

namespace OPL_WpfApp.easyTier
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OPL_WpfApp.easyTier"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:OPL_WpfApp.easyTier;assembly=OPL_WpfApp.easyTier"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:etinfo/>
    ///
    /// </summary>
    [TemplatePart(Name = PART_CopyButton, Type = typeof(Button))]
    public class etinfo : Control
    {
        static etinfo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(etinfo), new FrameworkPropertyMetadata(typeof(etinfo)));
        }
        private const string PART_CopyButton = "PART_CopyButton";

        //static NetworkInfoControl()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(
        //        typeof(NetworkInfoControl),
        //        new FrameworkPropertyMetadata(typeof(NetworkInfoControl)));
        //}

        //#region 依赖属性
        public static readonly DependencyProperty HostnameProperty =
           DependencyProperty.Register(
               "Hostname",
               typeof(string),
               typeof(etinfo),
               new PropertyMetadata("-"));

        public string Hostname
        {
            get { return (string)GetValue(HostnameProperty); }
            set { SetValue(HostnameProperty, value); }
        }
        // IP 地址（只读显示）
        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register(
                "IpAddress",
                typeof(string),
                typeof(etinfo),
                new PropertyMetadata("127.0.0.1"));

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        // 接收数据量（Rx）
        public static readonly DependencyProperty RxDataProperty =
            DependencyProperty.Register(
                "RxData",
                typeof(string),
                typeof(etinfo),
                new PropertyMetadata("0"));

        public string RxData
        {
            get { return (string)GetValue(RxDataProperty); }
            set { SetValue(RxDataProperty, value); }
        }

        // 发送数据量（Tx）
        public static readonly DependencyProperty TxDataProperty =
            DependencyProperty.Register(
                "TxData",
                typeof(string),
                typeof(etinfo),
                new PropertyMetadata("0"));

        public string TxData
        {
            get { return (string)GetValue(TxDataProperty); }
            set { SetValue(TxDataProperty, value); }
        }

        public static readonly DependencyProperty LatMsProperty =
            DependencyProperty.Register(
                "LatMs",
                typeof(string),
                typeof(etinfo),
                new PropertyMetadata("0"));

        public string LatMs
        {
            get { return (string)GetValue(LatMsProperty); }
            set { SetValue(LatMsProperty, value); }
        }
        //#endregion

        //#region 重写 OnApplyTemplate（用于绑定按钮事件）

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var copyButton = GetTemplateChild(PART_CopyButton) as Button;
            //Logger.Log("OnApplyTemplate");
            if (copyButton != null)
            {
                // 移除旧事件避免重复绑定
                //copyButton.Click -= OnCopyButtonClick;
                copyButton.Click += OnCopyButtonClick;
                
            }
            //Logger.Log("OnApplyTemplate");
        }

        //#endregion

        //#region 按钮点击事件：复制 IP 到剪贴板

        private void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            if(Copy_text(IpAddress))MessageBox.Show("已复制到剪贴板");
            try
            {
                Clipboard.SetText(IpAddress);
                // 可选：显示短暂提示（如 ToolTip 或动画）
                ToolTip = "已复制到剪贴板";
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => ToolTip = null));
            }
            catch (Exception ex)
            {
                // 在实际项目中可记录日志或弹出提示
                Logger.Log($"复制失败: {ex.Message}");
            }
        }
    }
}
