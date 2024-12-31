using Microsoft.Extensions.Hosting;
using OplWpf.Models;
using System.Net.Sockets;
using System.Text;

namespace OplWpf.Services;

public partial class HeartBeatService(HeartBeat heartBeat) : BackgroundService
{
    private const int KEEP_ALIVE_INTERVAL_SEC = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(KEEP_ALIVE_INTERVAL_SEC));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var tcpClient in heartBeat.Tcps)
            {
                if (tcpClient.Connected)
                {
                    const string keepAliveMessage = "\0";
                    var buffer = Encoding.ASCII.GetBytes(keepAliveMessage);
                    try
                    {
                        tcpClient.Send(buffer);
                    }
                    catch (SocketException)
                    {
                        //Logger.Log(se.Message);
                        //break;
                    }
                }
                else
                {

                }
            }

            foreach (var udpClient in heartBeat.Udps)
            {
                const string keepAliveMessage = "\0";
                byte[] buffer = Encoding.ASCII.GetBytes(keepAliveMessage);

                // 发送心跳包
                udpClient.Send(buffer, buffer.Length);
            }
        }
    }
}
