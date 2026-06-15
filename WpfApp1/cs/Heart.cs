using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OPL_WpfApp.Utils;

namespace userdata
{
    /// <summary>
    /// TCP 隧道保活客户端
    /// </summary>
    public class TcpClientWithKeepAlive
    {
        private const int KEEP_ALIVE_INTERVAL_SEC = 1;
        private readonly Socket _client;
        private Thread _keepAliveThread;
        private volatile bool _shouldStopKeepAlive = true;

        public TcpClientWithKeepAlive(string ipAddress, int port)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                _client.Connect(endPoint);

                Logger.Log($"[提示]TCP Connected to {ipAddress}:{port} 开启隧道保活");
                _shouldStopKeepAlive = false;
                _keepAliveThread = new Thread(SendKeepAliveMessage) { IsBackground = true };
                _keepAliveThread.Start();
            }
            catch (SocketException se)
            {
                Logger.Log($"SocketException: {se.Message}");
            }
        }

        private void SendKeepAliveMessage()
        {
            byte[] buffer = Encoding.ASCII.GetBytes("\x00");
            while (!_shouldStopKeepAlive)
            {
                if (_client.Connected)
                {
                    try
                    {
                        _client.Send(buffer);
                    }
                    catch (SocketException) { }
                }
                Thread.Sleep(TimeSpan.FromSeconds(KEEP_ALIVE_INTERVAL_SEC));
            }
        }

        public void StopSendingKeepAlive()
        {
            _shouldStopKeepAlive = true;
            try
            {
                if (_client.Connected)
                    _client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
            {
                Logger.Log($"SocketException: {ex.Message}");
            }
            finally
            {
                _client.Close();
            }
        }
    }

    /// <summary>
    /// UDP 隧道保活客户端
    /// </summary>
    public class UdpClientKeepAlive
    {
        private const int KEEP_ALIVE_INTERVAL_SEC = 1;
        private readonly UdpClient _udpClient;
        private Timer _keepAliveTimer;

        public UdpClientKeepAlive(string ipAddress, int port)
        {
            _udpClient = new UdpClient();
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                Logger.Log($"[提示]UDP Connected to {ipAddress}:{port} 开启隧道保活");
                StartSendingKeepAlive(remoteEP);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error: {ex.Message}");
            }
        }

        private void StartSendingKeepAlive(IPEndPoint remoteEP)
        {
            byte[] buffer = Encoding.ASCII.GetBytes("\x00");
            _keepAliveTimer = new Timer((state) =>
            {
                _udpClient.Send(buffer, buffer.Length, remoteEP);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(KEEP_ALIVE_INTERVAL_SEC));
        }

        public void StopSendingKeepAlive()
        {
            _keepAliveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _udpClient?.Close();
        }
    }
}
