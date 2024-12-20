﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using Serilog;

namespace OplWpf.Models;

public class TcpClientWithKeepAlive
{
    private const int KEEP_ALIVE_INTERVAL_SEC = 1; // 设置心跳间隔为1秒
    private readonly Socket _client;
    private readonly Thread? _keepAliveThread;
    private bool _shouldStopKeepAlive = true;
    private string ipAddress;
    private int port;

    public TcpClientWithKeepAlive(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            IPEndPoint endPoint = new(IPAddress.Parse(ipAddress), port);
            _client.Connect(endPoint);

            Log.Information("TCP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            _shouldStopKeepAlive = false;
            // 启动心跳线程
            _keepAliveThread = new(SendKeepAliveMessage)
            {
                IsBackground = true
            };
            _keepAliveThread.Start();
        }
        catch (SocketException se)
        {
            Log.Error(se, "SocketException");
        }
    }

    private void SendKeepAliveMessage()
    {
        while (!_shouldStopKeepAlive)
        {
            if (_client.Connected)
            {
                string keepAliveMessage = "\x00";
                byte[] buffer = Encoding.ASCII.GetBytes(keepAliveMessage);
                try
                {
                    _client.Send(buffer);
                }
                catch (SocketException)
                {
                    //Logger.Log(se.Message);
                    //break;
                }

                // 延迟至下一次心跳

            }
            else
            {
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                //_client.Connect(endPoint);
                //break;

            }
            Thread.Sleep(TimeSpan.FromSeconds(KEEP_ALIVE_INTERVAL_SEC));
        }
    }
    public void StopSendingKeepAlive()
    {
        _shouldStopKeepAlive = true;
        try
        {
            _client.Shutdown(SocketShutdown.Both); // 先关闭发送和接收
        }
        catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
        {
            // 处理可能的异常情况
        }
        finally
        {
            _client.Close(); // 然后关闭 Socket
        }
    }
}

public class UdpClientKeepAlive
{
    private const int KEEP_ALIVE_INTERVAL_SEC = 1; // 设置心跳间隔为10秒
    private readonly UdpClient _udpClient;
    private Timer? _keepAliveTimer;

    public UdpClientKeepAlive(string ipAddress, int port)
    {
        _udpClient = new UdpClient();
        try
        {
            var remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Log.Information("UDP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            // 开始发送心跳包
            StartSendingKeepAlive(remoteEP);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error");
        }
    }

    private void StartSendingKeepAlive(IPEndPoint remoteEP)
    {
        // 创建一个定时器来定期发送心跳包
        _keepAliveTimer = new Timer((state) =>
        {
            string keepAliveMessage = "\x00";
            byte[] buffer = Encoding.ASCII.GetBytes(keepAliveMessage);

            // 发送心跳包
            _udpClient.Send(buffer, buffer.Length, remoteEP);

            //Logger.Log("Sent UDP KeepAlive packet at {0}"+DateTime.Now.ToString("HH:mm:ss"));
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(KEEP_ALIVE_INTERVAL_SEC));
    }

    public void StopSendingKeepAlive()
    {
        // 停止发送心跳包并清理资源
        _keepAliveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _udpClient?.Close();
    }
}