using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System.Windows;
using static OPL_WpfApp.MainWindow;
using System.IO;

namespace userdata
{
    internal class Net
    {
        public async Task GetPreset()
        {
            Logger.Log("[执行]网络请求文件preset.json");
            string fileurl = "https://file.gldhn.top/file/json/preset.json";
            HttpClient httpClient = new HttpClient();
            try
            {
                // 发起GET请求
                
                HttpResponseMessage response = await httpClient.GetAsync(fileurl);

                // 检查响应状态是否成功
                if (response.IsSuccessStatusCode)
                {
                    // 获取响应内容的字符串形式
                    string contentString = await response.Content.ReadAsStringAsync();
                    wejson(contentString);
                    getjosn();
                    int v = presetss.version;
                    if (v > 5)
                    {
                        new Updata(presetss.upurl);
                        Logger.Log("[提示]获取预设完成,你的程序不是最新版本哦~ 开始后台下载更新包");
                    }
                    else 
                    { 
                        Logger.Log("[提示]获取预设完成,当前为最新版本~"); 
                    }
                    Uplog.Log(presetss.uplog); 
                }
                else
                {
                    Logger.Log($"[错误]请求失败，状态码：{response.StatusCode}");
                    //Console.WriteLine($"请求失败，状态码：{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("[错误]请求过程中发生错误：" + ex.Message);
            }
        }
        public void wejson(string ujson) //写入josn
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "preset.json");

            // 创建 bin 文件夹（如果不存在）
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            using (FileStream stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(ujson);
            }
        }
        public Presets presetss;
        public void getjosn()
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "preset.json");
            try
            {
                string jsonCont = File.ReadAllText(absolutePath);
                presetss = JsonConvert.DeserializeObject<Presets>(jsonCont);

            }
            catch (JsonException je)
            {
                // 如果JSON格式不正确，记录错误并返回null
                Logger.Log($"Error while deserializing JSON: {je.Message}");
            }

        }
    }
}
