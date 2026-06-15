using System;
using System.Windows;

namespace OPL_WpfApp.Utils
{
    /// <summary>
    /// 窗口辅助工具
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// 将窗口居中显示在屏幕中央
        /// </summary>
        public static void CenterOnScreen(Window window)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            window.Left = (screenWidth - window.Width) / 2;
            window.Top = (screenHeight - window.Height) / 2;
        }
    }
}
