using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static OPL_WpfApp.MainWindow_opl;

namespace userdata
{
    internal class Multicast
    {
        public Multicast() 
        {
            MulticastOpen = false;
        }
        private static bool MulticastOpen;
        private static int SrcPort = 0;
        public static void SetSrcPort(int port)
        {
            SrcPort = port;
        }
        public static void Stop()
        {
            MulticastOpen=false;
        }
        public static bool IsMulticastOpen()
        {
            return MulticastOpen;
        }
        public static async Task Seed()
        {
            MulticastOpen = true;
            if (SrcPort != 0 && MulticastOpen)
            {
                string multicastGroup = "224.0.2.60";
                int multicastPort = 4445;

                using (UdpClient client = new UdpClient(SrcPort))
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(multicastGroup), multicastPort);

                    byte[] ttl = new byte[] { 2 }; // 多播数据包的存活时间
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);

                    Logger.Log($"[提示]开启虚拟局域网发现 to:{SrcPort} 仅MC");

                    while (MulticastOpen )
                    {
                        string message = $"[MOTD]§2§l[OPL]§b远程世界 §7-by GLD[/MOTD][AD]{SrcPort}[/AD]";
                        byte[] data = Encoding.UTF8.GetBytes(message);

                        await client.SendAsync(data, data.Length, remoteEP);

                        await Task.Delay(1500); // 在发送下一个消息之前等待一段时间
                    }

                    Logger.Log($"[提示]关闭虚拟局域网发现 to:{SrcPort}");
                }
            }
            
        }
    }
}
