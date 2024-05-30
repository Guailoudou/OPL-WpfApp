using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static OPL_WpfApp.MainWindow;

namespace userdata
{
    internal class Updata
    {
        public Updata(string url, bool big = true)
        {
            if (big) 
            { 
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
                if (!File.Exists(absolutePath))
                {
                    _ = DmAsync(url, "nvb.zip"); //更新包 
                    _ = Dmupdata("https://file.gldhn.top/file/updata.exe"); //用来替换文件的程序
                }
            }
            else
            {
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.zip");
                if (!File.Exists(absolutePath))
                {
                    _ = DmAsync(url, "openp2p.zip"); //更新包 
                }
            }
        }
        public async Task DmAsync(string url,string name)
        {
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

                    Logger.Log($"[提示]ZIP更新文件已成功下载到：{savePath}");

                    MessageBox.Show("更新包下载完毕，推荐立刻重启软件，完成最后更新，防止出现BUG", "提示");


                }
                catch (HttpRequestException ex)
                {
                    Logger.Log($"下载失败: {ex.Message} 可尝试设置hosts来保障连接的可行性：\r\n172.64.32.5 file.gldhn.top\r\n172.64.32.5 blog.gldhn.top");
                }
                catch (IOException ex)
                {
                    Logger.Log($"文件操作失败: {ex.Message}");
                }
            }
        }
        public async Task Dmupdata(string url)
        {
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updata.exe");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            //Thread.Sleep(2000);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送GET请求获取ZIP文件的字节流
                    byte[] fileBytes = await client.GetByteArrayAsync(url);

                    // 将字节流写入到本地文件
                    File.WriteAllBytes(savePath, fileBytes);

                    Logger.Log($"[提示]更新程序已成功下载到：{savePath}");
                }
                catch (HttpRequestException ex)
                {
                    Logger.Log($"下载失败: {ex.Message}");
                }
                catch (IOException ex)
                {
                    Logger.Log($"文件操作失败: {ex.Message}");
                }
            }
        }
    }
}
