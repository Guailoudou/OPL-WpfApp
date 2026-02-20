using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OPL_WpfApp.MainWindow_opl;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.AxHost;
using System.Windows;
using userdata;

namespace OPL_WpfApp.Openp2p
{
    internal class Openp2pService
    {
        public delegate void CheckLogCallback(string result);

        private Process process;
        public static List<UdpClientKeepAlive> udps = new List<UdpClientKeepAlive>();
        public static List<TcpClientWithKeepAlive> tcps = new List<TcpClientWithKeepAlive>();

        public void Run(CheckLogCallback check)
        {

            // 创建进程对象
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", opname); // 控制台应用路径
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            //startInfo.Arguments = "-d --network-name " + linkname + "";
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true; // 不显示新的命令行窗口

            process = new Process();
            process.StartInfo = startInfo;
            // 设置输出数据接收事件
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Application.Current.Dispatcher.Invoke(() => // 必须在UI线程更新RichTextBox内容
                    {
                        //richOutput.AppendText(e.Data + Environment.NewLine);
                        //richOutput.Text = e.Data + Environment.NewLine + richOutput.Text;
                        //richOutput.ScrollToEnd(); 



                        check(e.Data);
                    });
                }
            });
            bool tl = true;
            // 设置错误数据接收事件
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Logger.Log("【错误】: " + e.Data + Environment.NewLine);
                        if (tl)
                        {

                            if (process != null && !process.HasExited)
                            {
                                process.CancelOutputRead();
                                process.CancelErrorRead();
                                process.Kill();

                                //if (udps != null) foreach (UdpClientKeepAlive app in udps) app.StopSendingKeepAlive();
                                if (tcps != null) foreach (TcpClientWithKeepAlive app in tcps) app.StopSendingKeepAlive();
                            }
                            Stop();
                            Logger.Log("主程序程序openp2p崩掉了！请查看软件状态，尝试重新启动，或联系作者", "错误");
                            tl = false;
                            //_ = DelayAndExecute();
                        }
                    });
                }
            });

            // 启动进程并开始接收输出
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

            }
            catch (Exception ex)
            {

                Logger.Log("[错误]启动失败，看来被安全中心拦截" + ex.ToString());
                MessageBox.Show("启动失败，可能被安全中心拦截了，请尝试添加排除后重新启动\r可以点击本软件设置页面右上角自动添加排除按钮后重试\r内网穿透程序常被黑客用来用来入侵企业内网，故非常容易报毒，请信任程序的安全性\r\r请进行如下操作：Windows安全中心->病毒和威胁防护->“病毒和威胁防护”设置->管理设置->排除项->添加或删除排除项->添加排除项->文件夹  添加以下路径\r" + System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin") + "\r" + AppDomain.CurrentDomain.BaseDirectory + "\n\n如果还是不行请进行如下尝试\r\nWindows安全中心->应用和浏览器控制->智能应用控制设置->关闭", "警告");
                if (process != null)
                    if (!process.HasExited)
                        process.Kill();
                Stop();
                return;
            }

        }
        public void Stop()
        {

        }


    }
}
