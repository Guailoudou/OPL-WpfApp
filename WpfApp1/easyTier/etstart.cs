using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using static OPL_WpfApp.MainWindow_opl;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using userdata;
using System.IO;
using System.Xml.Schema;

namespace OPL_WpfApp.easyTier
{
    internal class etstart
    {
        public static Process process;
        private string name = "easytier-core.exe";
        private string infoname = "easytier-cli.exe";
        private MainWindow_opl mainWindow;
        private string linkname = "";
        public etstart(MainWindow_opl mainWindow)
        {
            this.mainWindow = mainWindow;
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "easytier-windows-x86_64", name)) || !File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "easytier-windows-x86_64", infoname)))
                new Updata(Net.Getmirror("https://file.gldhn.top/file/easytier-windows-x86_64-v2.3.2.zip"), "easytier.zip", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"), true);
        }
        public void Open()
        {
            if (mainWindow.on)
            {
                MessageBox.Show("请先关闭主程序再启动，本模块与原始模块独立", "警告");
                return;
            }
            mainWindow.on = true;
            mainWindow.eton = true;
            // 创建进程对象
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "easytier-windows-x86_64", name); // 控制台应用路径
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            startInfo.Arguments = "-d --network-name " + linkname + " --network-secret " + linkname + " -p tcp://public.easytier.cn:11010";
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true; // 不显示新的命令行窗口
            // 启动进程并开始接收输出
            
                process = new Process();
            process.StartInfo = startInfo;
            // 设置输出数据接收事件
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    mainWindow.Dispatcher.Invoke(() => 
                    {
                        //richOutput.AppendText(e.Data + Environment.NewLine);
                        mainWindow.richOutput.Text = e.Data + Environment.NewLine + mainWindow.richOutput.Text;
                        //richOutput.ScrollToEnd(); 



                        //Checkopen(e.Data);
                    });
                }
            });
            //bool tl = true;
            // 设置错误数据接收事件
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {

                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        Logger.Log("【错误】: " + e.Data + Environment.NewLine);
                        //if (tl)
                        //{

                        //    if (process != null && !process.HasExited)
                        //    {
                        //        process.CancelOutputRead();
                        //        process.CancelErrorRead();
                        //        process.Kill();

                        //        //if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
                        //        if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
                        //    }
                        //    Stop();
                        //    Logger.Log("主程序程序openp2p崩掉了！请查看软件状态，尝试重新启动，或联系作者", "错误");
                        //    tl = false;
                        //    _ = DelayAndExecute();
                        //}
                    });
                }
            });

            
                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    _ = DelayCheck();
                    MessageBox.Show("网络启动成功，请等待大约5-8s，左侧会出现连接到网络的设备，如果你是创建网络的，请发送你的uid给其他用户加入网络");
                    //process.WaitForExit();
                    //Logger.Log("网络已关闭奥");
                    //Stop();

                }
                catch (Exception ex)
                {

                    Logger.Log("[错误]启动失败，看来被安全中心拦截" + ex.ToString());
                    MessageBox.Show("启动失败");
                    if (process != null)
                        if (!process.HasExited)
                            process.Kill();
                    //Stop();
                    mainWindow.on = false;
                    mainWindow.eton = false;
                    return;
                }
            
            

        }

        public void Stop()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.CancelOutputRead();
                    process.CancelErrorRead();
                    process.Kill();
                }
            }
            mainWindow.NetworkList.Items.Clear();
            mainWindow.on=false;
            mainWindow.eton = false;
        }
        public void setlinkname(string linkname)
        {
            this.linkname = linkname;
        }
        async Task DelayCheck()
        {
            
           
            
            while (mainWindow.on)
            {
                await Task.Delay(5 * 1000);
                //Logger.Log("正在检查节点状态...");
                string output = "";
                Action actionOnMainThread = () =>
                {
                    ProcessStartInfo Info = new ProcessStartInfo();
                    Info.FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "easytier-windows-x86_64", infoname); // 控制台应用路径
                    Info.RedirectStandardOutput = true;
                    Info.StandardOutputEncoding = Encoding.UTF8;
                    Info.StandardErrorEncoding = Encoding.UTF8;
                    Info.Arguments = "peer";
                    Info.RedirectStandardError = true;
                    Info.UseShellExecute = false;
                    Info.CreateNoWindow = true; // 不显示新的命令行窗口

                    Process pro = new Process();
                    pro.StartInfo = Info;
                    int count = 0;
                    // 设置输出数据接收事件
                    pro.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (count % 2 != 0) {
                                if (output == "") output = e.Data;
                                else output += (Environment.NewLine + e.Data);
                            }
                            count++;
                            
                        }
                    });
            

                    //try
                    //{
                        pro.Start();
                        pro.BeginOutputReadLine();
                        pro.WaitForExit();
                        //Logger.Log("【节点状态】: " + output + Environment.NewLine);
                        NetworkNode[] node = null;
                        try
                        {
                            node = TableParser.ParseTable(output);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("[错误]获取失败" + ex.ToString());
                            Stop();
                            //MessageBox.Show("出现错误，请重试");
                            return;
                        }
                        if (node != null)
                        {
                            //Logger.Log("[成功]解析成功" + node.ToString());
                            mainWindow.NetworkList.Items.Clear();
                            foreach (NetworkNode n in node)
                            {
                                Brush bb;
                            //string rx = "0",tx = "0";
                                if (n.Cost == "Local")
                                {
                                    bb = System.Windows.Media.Brushes.Green;
                                }
                                else
                                {
                                    bb = System.Windows.Media.Brushes.DeepSkyBlue;
                                }
                            //try
                            //{
                            //rx = n.RxBytes;

                            //}
                            //catch
                            //{
                            //    rx = 0;
                            //}
                            //try
                            //{
                            //   tx = long.Parse(n.TxBytes);

                            //}
                            //catch
                            //{
                            //    tx = 0;
                            //}
                            if (n.Hostname == "p2p") continue;
                            string IP = "-";
                            if (n.Ipv4.Length > 3)
                            {
                                IP = n.Ipv4.Substring(0, n.Ipv4.Length - 3);
                            }
                                var networkInfo = new etinfo
                                {
                                    Hostname = n.Hostname,
                                    IpAddress = IP,
                                    RxData = n.RxBytes,
                                    TxData = n.TxBytes,
                                    Background = System.Windows.Media.Brushes.White,
                                    BorderBrush = bb,
                                    BorderThickness = new System.Windows.Thickness(1),
                                    Margin = new Thickness(5),
                                    LatMs = n.LatMs + "ms"
                                };
                                mainWindow.NetworkList.Items.Add(networkInfo);
                            }
                        }

                    //}
                    //catch (Exception ex)
                    //{

                    //    Logger.Log("[错误]解析失败"+ex.Message);
                   
                    //}

                };
                if (SynchronizationContext.Current != null)
                {
                    // 如果存在同步上下文，则使用它来调度操作
                    SynchronizationContext.Current.Post(state => actionOnMainThread(), null);
                }
                else
                {
                    // 如果没有同步上下文（例如控制台应用程序），则直接调用
                    actionOnMainThread();
                }
            }
            

            //// 假设这是一个需要在主线程上执行的方法
            //Action actionOnMainThread = () =>
            //{
            //    // 调用主进程中的方法
            //    Strapp();
            //};

            //if (SynchronizationContext.Current != null)
            //{
            //    // 如果存在同步上下文，则使用它来调度操作
            //    SynchronizationContext.Current.Post(state => actionOnMainThread(), null);
            //}
            //else
            //{
            //    // 如果没有同步上下文（例如控制台应用程序），则直接调用
            //    actionOnMainThread();
            //}
        }
    }
}
