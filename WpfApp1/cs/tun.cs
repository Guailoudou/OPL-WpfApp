//using System;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Net.NetworkInformation;
//using System.Diagnostics;
//using System.Text;
//using System.Threading;
//using System.Linq;
//using System.Net;

//public class VirtualTun
//{
//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunCreateAdapter(out IntPtr adapter, string name, IntPtr guid, uint mtu);

//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunStartReceiving(IntPtr adapter);

//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunReceivePacket(IntPtr adapter, out byte[] packet, out uint bytesRead);

//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunSendPacket(IntPtr adapter, byte[] packet, uint length, uint flags);

//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunStopReceiving(IntPtr adapter);

//    [DllImport("wintun.dll", SetLastError = true)]
//    private static extern uint WintunDestroyAdapter(IntPtr adapter);

//    private const string TunIfaceName = "optun-beta";

//    public static void main()
//    {
//        IntPtr adapter = IntPtr.Zero;

//        try
//        {
//            // 创建 TUN 接口
//            CreateTunDevice(TunIfaceName, out adapter);

//            // 设置 IP 地址
//            SetTunAddress(TunIfaceName, "10.26.35.1/24");

//            // 开始接收数据包
//            StartReceiving(adapter);

//            // 循环接收数据包
//            ReceivePackets(adapter);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error: {ex.Message}");
//        }
//        finally
//        {
//            // 清理资源
//            StopReceiving(adapter);
//            DestroyAdapter(adapter);
//        }
//    }

//    private static void CreateTunDevice(string tunName, out IntPtr adapter)
//    {
//        uint status = WintunCreateAdapter(out adapter, tunName, IntPtr.Zero, 1420);
//        if (status != 0)
//        {
//            throw new Exception($"Failed to create adapter: {status}");
//        }
//    }

//    private static void SetTunAddress(string ifname, string localAddr)
//    {
//        var parts = localAddr.Split('/');
//        var ipAddress = parts[0];
//        var subnetMask = parts[1];

//        // 使用 netsh 命令来设置 IP 地址
//        var setIpCommand = $"netsh interface ip set address name={ifname} source=static addr={ipAddress} mask={subnetMask}";

//        ProcessStartInfo psi = new ProcessStartInfo("cmd", $"/C {setIpCommand}")
//        {
//            RedirectStandardOutput = true,
//            UseShellExecute = false,
//            CreateNoWindow = true
//        };

//        using (Process process = new Process())
//        {
//            process.StartInfo = psi;
//            process.Start();

//            string output = process.StandardOutput.ReadToEnd();
//            process.WaitForExit();

//            if (process.ExitCode != 0)
//            {
//                throw new Exception($"Failed to set IP address: {output}");
//            }
//        }

//        Console.WriteLine($"Set IP address: {localAddr} on interface: {ifname}");
//    }

//    private static void StartReceiving(IntPtr adapter)
//    {
//        uint status = WintunStartReceiving(adapter);
//        if (status != 0)
//        {
//            throw new Exception($"Failed to start receiving: {status}");
//        }
//    }

//    private static void ReceivePackets(IntPtr adapter)
//    {
//        byte[] packet;
//        uint bytesRead;

//        while (true)
//        {
//            uint status = WintunReceivePacket(adapter, out packet, out bytesRead);
//            if (status != 0)
//            {
//                throw new Exception($"Failed to receive packet: {status}");
//            }

//            // 处理接收到的数据包
//            Console.WriteLine($"Received packet size: {bytesRead}");

//            // 发送数据包回接口
//            SendPacket(adapter, packet, (uint)bytesRead);
//        }
//    }

//    private static void SendPacket(IntPtr adapter, byte[] packet, uint length)
//    {
//        uint status = WintunSendPacket(adapter, packet, length, 0);
//        if (status != 0)
//        {
//            throw new Exception($"Failed to send packet: {status}");
//        }
//    }

//    private static void StopReceiving(IntPtr adapter)
//    {
//        uint status = WintunStopReceiving(adapter);
//        if (status != 0)
//        {
//            throw new Exception($"Failed to stop receiving: {status}");
//        }
//    }

//    private static void DestroyAdapter(IntPtr adapter)
//    {
//        uint status = WintunDestroyAdapter(adapter);
//        if (status != 0)
//        {
//            throw new Exception($"Failed to destroy adapter: {status}");
//        }
//    }
//}