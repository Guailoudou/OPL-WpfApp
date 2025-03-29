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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
public class tunnel
{
    private static readonly string userDirectory = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "bin");
    private Thread logPrintingThread, transferUpdateThread;
    private readonly string configFile = Path.Combine(userDirectory, "opltun.conf");
    private Tunnel.Ringlogger log;
    private readonly string logFile = Path.Combine(userDirectory, "log.bin");
    private string config = "";
    private volatile bool threadsRunning;
    private volatile bool isRunning = false;
    //private TextBox logBox;
    private Label tunspeed;
    tunconfig tunconfig = new tunconfig();
    public void csh(Label tunspeed)
    {
        //this.logBox = logBox;
        this.tunspeed = tunspeed;
        Directory.CreateDirectory(userDirectory);
        log = new Tunnel.Ringlogger(logFile, "GUI");
        
        new Updata(Net.Getmirror("https://file.gldhn.top/file/json/wireguard_keys.json"), "wgkey.json");
        new Updata(Net.Getmirror("https://file.gldhn.top/file/dll/tunnel.dll"), "tunnel.dll", AppDomain.CurrentDomain.BaseDirectory);
        new Updata(Net.Getmirror("https://file.gldhn.top/file/dll/wireguard.dll"), "wireguard.dll", AppDomain.CurrentDomain.BaseDirectory);
    }
    public void OpenTunnel(Button button,int id,int port)
    {
        if (!isRunning) { 
            config = tunconfig.buildconfig(id == 1 ? true : false, port,id);
            if(config=="err") return;
            try
                {
                
                    threadsRunning = true;
                    isRunning = true;
                    //logBox.Text = "";
                    if(button.Content.ToString() == "连接网络") button.Content = "断开连接";
                    else button.Content = "关闭网络";
                    //logPrintingThread = new Thread(new ThreadStart(tailLog));
                    transferUpdateThread = new Thread(new ThreadStart(tailTransfer));
                    //logPrintingThread.Start();
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
                    isRunning = false;
                    //try { File.Delete(configFile); } catch { }
                }
            }
        else { 
            try
            {
                threadsRunning = false;
                isRunning = false;
                if (button.Content.ToString() == "断开连接") button.Content = "连接网络";
                else button.Content = "创建/开启网络";
                //logPrintingThread.Interrupt();
                transferUpdateThread.Interrupt();
                //try { logPrintingThread.Join(); } catch { }
                try { transferUpdateThread.Join(); } catch { }
                Tunnel.Service.Remove(configFile, true);
                try { File.Delete(configFile); } catch { }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                try { File.Delete(configFile); } catch { }
            }
        }
    }
    public void SetConfig(string config)
    {
        this.config = config;
    }

    //private void tailLog()
    //{
    //    var cursor = Tunnel.Ringlogger.CursorAll;
    //    while (threadsRunning)
    //    {
    //        var lines = log.FollowFromCursor(ref cursor);
    //        foreach (var line in lines)
    //            //Logger.Log(line);
    //            logBox.Dispatcher.Invoke(()=> {
    //                logBox.Text = line + "\r\n" + logBox.Text;
    //            });
    //            //new Action<string>(Logger.Log);
    //        try
    //        {
    //            Thread.Sleep(300);
    //        }
    //        catch
    //        {
    //            break;
    //        }
    //    }
    //}

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