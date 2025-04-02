using Microsoft.Extensions.Hosting;
using OplWpf.Models;
using System.Net.Sockets;
using System.Text;

namespace OplWpf.Services;

public partial class HeartBeatService(HeartBeat heartBeat) : BackgroundService
{
    private const int KeepAliveIntervalSec = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string keepAliveMessage = "\0";
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(KeepAliveIntervalSec));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var tcpClient in heartBeat.Tcps)
                {
                    if (tcpClient.Connected)
                    {
                        var buffer = Encoding.ASCII.GetBytes(keepAliveMessage);
                        try
                        {
                            tcpClient.GetStream().Write(buffer);
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
                    var buffer = Encoding.ASCII.GetBytes(keepAliveMessage);

                    // 发送心跳包
                    await udpClient.SendAsync(buffer, buffer.Length);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}