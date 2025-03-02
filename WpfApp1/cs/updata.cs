using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static OPL_WpfApp.MainWindow_opl;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
namespace userdata
{
    internal class Updata
    {
        public Updata(string url,string SaveName)
        {
            
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", SaveName);
            if (!File.Exists(absolutePath))
            {
                _ = Dmfile(url, SaveName); //更新包 
                if(SaveName== "nvb.zip")
                    _ = Dmfile(Net.Getmirror("https://file.gldhn.top/file/updata.exe"), "updata.exe");
            }

        }
        public async Task Dmfile(string url,string name)
        {
            Logger.Log($"[提示]开始下载文件：{url} 保存名 ：{name}");
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", name);
            //Thread.Sleep(2000);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送GET请求获取ZIP文件的字节流
                    byte[] fileBytes = await client.GetByteArrayAsync(url);

                    // 将字节流写入到本地文件
                    File.WriteAllBytes(savePath, fileBytes);

                    Logger.Log($"[提示]文件已成功下载到：{savePath}");
                    if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updata.exe")) && name == "nvb.zip")
                        MessageBox.Show("已完成更新文件下载，建议重启以完成最后更新！", "提示");
                    if(name== "openp2p.zip" || name == "openp2p23.zip")
                    {
                        string saveOPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", name);
                        if (File.Exists(saveOPath))
                        {
                            OPL_WpfApp.App.ExtractZipAndOverwrite(saveOPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));
                        }
                        over = true;
                        Logger.Log($"[提示]已完成关键文件下载！可以启动程序了");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.Log($"下载失败: {ex.Message} ");
                    if (name == "openp2p.zip")
                    {
                        MessageBox.Show("关键文件下载失败，你可以尝试重启或者下载压缩包版本，如果你之前可以正常启动，可以在设置关闭openp2p文件校验后重启", "错误");
                    }
                }
                catch (IOException ex)
                {
                    Logger.Log($"文件操作失败: {ex.Message}");
                }
            }
        }
        //public async Task Dmfile(string url,string saveName)
        //{
        //    string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveName);
        //    if (File.Exists(savePath))
        //    {
        //        File.Delete(savePath);
        //    }
        //    //Thread.Sleep(2000);
        //    using (HttpClient client = new HttpClient())
        //    {
        //        try
        //        {
        //            // 发送GET请求获取ZIP文件的字节流
        //            byte[] fileBytes = await client.GetByteArrayAsync(url);

        //            // 将字节流写入到本地文件
        //            File.WriteAllBytes(savePath, fileBytes);

        //            Logger.Log($"[提示]已成功下载文件到：{savePath}");
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            Logger.Log($"下载失败: {ex.Message}");
        //        }
        //        catch (IOException ex)
        //        {
        //            Logger.Log($"文件操作失败: {ex.Message}");
        //        }
        //    }
        //}
    }
}
