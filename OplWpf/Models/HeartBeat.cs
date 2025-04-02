using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;

namespace OplWpf.Models;

[Injection(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
public class HeartBeat(ILogger<HeartBeat> logger)
{
    private readonly List<TcpClient> _tcps = [];
    private readonly List<UdpClient> _udps = [];

    public IReadOnlyList<TcpClient> Tcps => _tcps;
    public IReadOnlyList<UdpClient> Udps => _udps;

    public void AddTcp(string ipAddress, int port)
    {
        var client = new TcpClient(AddressFamily.InterNetwork);
        try
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            client.Connect(endPoint);
            logger.LogInformation("TCP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            // 启动心跳线程
            _tcps.Add(client);
        }
        catch (SocketException se)
        {
            logger.LogError(se, "SocketException");
        }
    }

    public void ClearTcp()
    {
        foreach (var tcpClient in _tcps)
        {
            tcpClient.Close(); // 然后关闭 Socket
        }

        _tcps.Clear();
    }

    public void AddUdp(string ipAddress, int port)
    {
        try
        {
            var remoteEp = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var udpClient = new UdpClient(remoteEp);
            logger.LogInformation("UDP Connected to {ipAddress}:{port}开启隧道保活", ipAddress, port);
            // 开始发送心跳包
            _udps.Add(udpClient);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error");
        }
    }

    public void ClearUdp()
    {
        foreach (var udpClient in _udps)
        {
            udpClient.Close();
        }

        _udps.Clear();
    }
}