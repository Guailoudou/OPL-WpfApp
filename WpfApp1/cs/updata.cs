using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static OPL_WpfApp.MainWindow;

namespace userdata
{
    internal class Updata
    {
        public Updata(string url)
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
            if (!File.Exists(absolutePath))
            {
                _ = DmAsync(url);
            }
        }
        public async Task DmAsync(string url)
        {
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
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
