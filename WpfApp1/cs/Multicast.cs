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
        private static string multicastGroup = "224.0.2.60";
        private static int multicastPort = 4445;
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
                
                using (UdpClient client = new UdpClient(0))
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
        public bool _isRunning = false;
        private UdpClient _udpClient;
        public event EventHandler<string> DataReceived;
        public async Task StartListeningAsync()
        {
            try
            {
                _udpClient = new UdpClient(multicastPort);
                var groupEP = new IPEndPoint(IPAddress.Parse(multicastGroup), 0);
                _udpClient.JoinMulticastGroup(groupEP.Address);
                _isRunning = true;
                Logger.Log("开始监听多播消息...");

                while (_isRunning)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    //Logger.Log($"接收到的消息: {receivedMessage}");
                    OnDataReceived(receivedMessage);
                    // 停止监听
                    StopListening();
                    _isRunning = false;
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"已关闭监听: {ex.Message}");
            }
            finally
            {
                _udpClient?.Close();
            }
        }
        protected virtual void OnDataReceived(string message)
        {
            DataReceived?.Invoke(this, message);
        }
        public void StopListening()
        {
            _isRunning = false;
            _udpClient?.Close();
            Logger.Log("停止监听多播消息...");
        }
    }
}
