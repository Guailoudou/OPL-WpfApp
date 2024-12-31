using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;

namespace OplWpf.Models;

public class HeartBeat(ILogger<HeartBeat> logger)
{
    private readonly List<Socket> tcps = [];
    private readonly List<UdpClient> udps = [];

    public IReadOnlyList<Socket> Tcps => tcps;
    public IReadOnlyList<UdpClient> Udps => udps;

    public void AddTcp(string ipAddress, int port)
    {
        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            client.Connect(endPoint);
            logger.LogInformation("TCP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            // 启动心跳线程
            tcps.Add(client);
        }
        catch (SocketException se)
        {
            logger.LogError(se, "SocketException");
        }
    }

    public void ClearTcp()
    {
        foreach (var tcpClient in tcps)
        {
            try
            {
                tcpClient.Shutdown(SocketShutdown.Both); // 先关闭发送和接收
            }
            catch (Exception ex)
            {
                // 处理可能的异常情况
            }
            finally
            {
                tcpClient.Close(); // 然后关闭 Socket
            }
        }
        tcps.Clear();
    }

    public void AddUdp(string ipAddress, int port)
    {
        try
        {
            var remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var udpClient = new UdpClient(remoteEP);
            logger.LogInformation("UDP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            // 开始发送心跳包
            udps.Add(udpClient);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error");
        }
    }

    public void ClearUdp()
    {
        foreach (var udpClient in udps)
        {
            udpClient.Close();
        }
        udps.Clear();
    }
}
