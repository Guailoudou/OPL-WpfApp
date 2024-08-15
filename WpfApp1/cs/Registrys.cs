using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace userdata
{
    internal class Registrys
    {
        static string appName = "OPL";
        static string appPath = System.IO.Path.Combine(Process.GetCurrentProcess().MainModule.FileName);
        public static void AddToStartup()
        {
            // 打开注册表中的Run键
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (rk == null)
                {
                    Console.WriteLine("Run key not found.");
                    return;
                }

                // 添加程序路径到注册表
                rk.SetValue(appName, appPath);

                Console.WriteLine($"Application {appName} added to startup.");
            }
        }

        public static void RemoveFromStartup()
        {
            // 打开注册表中的Run键
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (rk == null)
                {
                    Console.WriteLine("Run key not found.");
                    return;
                }

                // 删除指定的程序名
                if (rk.GetValue(appName) != null)
                {
                    rk.DeleteValue(appName, false);
                    Console.WriteLine($"Application {appName} removed from startup.");
                }
                else
                {
                    Console.WriteLine($"Application {appName} is not in the startup list.");
                }
            }
        }

        public static bool IsInStartup()
        {
            // 打开注册表中的Run键
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (rk == null)
                {
                    return false;
                }

                // 检查指定的程序名是否存在
                if (rk.GetValue(appName) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
