using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPL_WpfApp.cs
{
    internal class Logs
    {
        public static void Out_Logs(string log) {
            GL.Logs_Str += $"\n[{DateTime.Now.ToString("HH:mm:ss")}]"+log;
        }
    }
}
