using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using userdata;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
namespace OPL_WpfApp
{
    public partial class MainWindow_opl : Window
    {
        private void SetThene(string theme)
        {
            set set = new set();
            set.settings.Theme = theme;
            set.Write();
        }
        private void GetTheme()
        {
            set set = new set();
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
    }
}
