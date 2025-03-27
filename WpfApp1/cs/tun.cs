//using System;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Net.NetworkInformation;
//using System.Diagnostics;
//using System.Text;
//using System.Threading;
//using System.Linq;
//using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Path = System.IO.Path;
using Tunnel;
using userdata;
using System;
using System.Runtime.Remoting.Messaging;
using static OPL_WpfApp.MainWindow_opl;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Shapes;

public class tunnel
{
    private static readonly string userDirectory = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Config");
    private Thread logPrintingThread, transferUpdateThread;
    private readonly string configFile = Path.Combine(userDirectory, "demobox.conf");
    private Tunnel.Ringlogger log;
    private readonly string logFile = Path.Combine(userDirectory, "log.bin");
    private readonly string config = "[Interface]\r\nPrivateKey = eNOlN1FvS/uvB+oT+wRLolxYEpiFnjlMkgKia4SDkFI=\r\nListenPort = 50814\r\nAddress = 10.0.8.1/24\r\n\r\n[Peer]\r\nPublicKey = bZCHopp+A//TakQhj/e7QULfzWJiQSonUjQNgGODdHI=\r\nAllowedIPs = 10.0.8.0/24, 224.0.0.0/8\r\nPersistentKeepalive = 1\r\n\r\n[Peer]\r\nPublicKey = cMZDRP/dx+03ssNiEcIvqebzWXvS6XWMHPXN0DCQZBU=\r\nAllowedIPs = 10.0.8.0/24, 224.0.0.0/8\r\nPersistentKeepalive = 1\r\n";
    private volatile bool threadsRunning;
    private volatile bool isRunning = false;
    private TextBox logBox;
    private Label tunspeed;
    public void csh(TextBox logBox,Label tunspeed)
    {
        this.logBox = logBox;
        this.tunspeed = tunspeed;
        Directory.CreateDirectory(userDirectory);
        log = new Tunnel.Ringlogger(logFile, "GUI");
        logPrintingThread = new Thread(new ThreadStart(tailLog));
        transferUpdateThread = new Thread(new ThreadStart(tailTransfer));

    }
    public void OpenTunnel()
    {
        
        try
        {
            threadsRunning = true;
            logPrintingThread.Start();
            transferUpdateThread.Start();
            using (FileStream stream = new FileStream(configFile, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(config);
            }
            //Tunnel.Service.Remove(configFile, false);
            Service.Add(configFile, true);
            //await Task.Run(() => Tunnel.Service.Add(configFile, true));
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            //try { File.Delete(configFile); } catch { }
        }
    }
    public async void CloseTunnel()
    {
        try
        {
            threadsRunning = false;
            logPrintingThread.Interrupt();
            transferUpdateThread.Interrupt();
            try { logPrintingThread.Join(); } catch { }
            try { transferUpdateThread.Join(); } catch { }
            await Task.Run(() => Tunnel.Service.Remove(configFile, true));
            try { File.Delete(configFile); } catch { }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            try { File.Delete(configFile); } catch { }
        }
    }

    private void tailLog()
    {
        var cursor = Tunnel.Ringlogger.CursorAll;
        while (threadsRunning)
        {
            var lines = log.FollowFromCursor(ref cursor);
            foreach (var line in lines)
                //Logger.Log(line);
                logBox.Dispatcher.Invoke(new Action<string>(logBox.AppendText), new object[] { line + "\r\n" });
                //new Action<string>(Logger.Log);
            try
            {
                Thread.Sleep(300);
            }
            catch
            {
                break;
            }
        }
    }

    private void tailTransfer()
    {
        Tunnel.Driver.Adapter adapter = null;
        while (threadsRunning)
        {
            if (adapter == null)
            {
                while (threadsRunning)
                {
                    try
                    {
                        adapter = Tunnel.Service.GetAdapter(configFile);
                        break;
                    }
                    catch
                    {
                        try
                        {
                            Thread.Sleep(1000);
                        }
                        catch { }
                    }
                }
            }
            if (adapter == null)
                continue;
            try
            {
                ulong rx = 0, tx = 0;
                var config = adapter.GetConfiguration();
                foreach (var peer in config.Peers)
                {
                    rx += peer.RxBytes;
                    tx += peer.TxBytes;
                }
                //Logger.Log(String.Format("{0} RX, {1} TX", rx, tx));
                tunspeed.Dispatcher.Invoke(() =>
                {
                    tunspeed.Content = String.Format("{0} RX, {1} TX", rx, tx);
                });
                //(new Action<string>(tunspeed.SetContent), new object[] { String.Format("{0} RX, {1} TX", rx, tx) });
                Thread.Sleep(1000);
            }
            catch { adapter = null; }
        }
    }
}