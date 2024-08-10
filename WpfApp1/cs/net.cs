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
using System.Security.Cryptography;
using static OPL_WpfApp.MainWindow;
using System.IO;
using System.Windows.Controls;
using System.Net.Sockets;
using OPL_WpfApp.cs;
namespace userdata
{
    internal class Net
    {
        private static readonly int pvn = 31;//协议版本号
        public static int Getpvn()
        {
            return pvn;
        }
        public static bool ismirror = true;
        public async Task GetPreset()
        {
            string isgitee = ismirror ? "gitee镜像" : "";
            Logs.Out_Logs($"[执行]网络请求文件preset.json-{isgitee}");
            string fileurl = "https://file.gldhn.top/file/json/preset.json"; //http://127.0.0.1:85/file/json/preset.json https://file.gldhn.top/file/json/preset.json
            fileurl = Getmirror(fileurl);
            HttpClient httpClient = new HttpClient();
            try
            {
                // 发起GET请求
                //_ = httpClient.SendAsync(new HttpRequestMessage
                //{
                //    Method = new HttpMethod("HEAD"),
                //    RequestUri = new Uri("https://file.gldhn.top/")
                //});

                HttpResponseMessage response = await httpClient.GetAsync(fileurl);

                // 检查响应状态是否成功
                if (response.IsSuccessStatusCode)
                {
                    // 获取响应内容的字符串形式
                    string contentString = await response.Content.ReadAsStringAsync();
                    wejson(contentString);
                    getjosn();
                    int v = presetss.version;
                    string ophash = presetss.ophash;
                    string opurl = presetss.opurl;
                    string opPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "openp2p.exe");
                    if (v > pvn)
                    {
                        new Updata(Getmirror(presetss.upurl));
                        Logs.Out_Logs($"[提示]获取预设完成,你的程序不是最新版本哦~ 开始后台下载更新包-{isgitee}");
                    }
                    else
                    {
                        Logs.Out_Logs($"[提示]获取预设完成,当前为最新版本~ {isgitee}");
                    }
                    if ((CalculateMD5Hash(opPath) != ophash && ophash != null) || !File.Exists(opPath))
                    {
                        new Updata(Getmirror(presetss.opurl), false);
                        Logs.Out_Logs("[提示]你的openp2p不是最新版本哦~ 开始后台下载更新包");
                    }
                    Uplog.Log(presetss.uplog);
                }
                else
                {
                    Logs.Out_Logs($"[错误]请求{fileurl}失败，状态码：{response.StatusCode}  可尝试设置hosts来保障连接的可行性：\n172.64.32.5 file.gldhn.top\n172.64.32.5 blog.gldhn.top");
                    //Console.WriteLine($"请求失败，状态码：{response.StatusCode}");
                    Uplog.Log("获取失败");
                    if (ismirror)
                    {
                        ismirror = false;
                        await GetPreset();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Logs.Out_Logs($"[错误]HTTP请求异常 HttpRequestException: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Logs.Out_Logs($"[错误]请求被取消 TaskCanceledException: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Logs.Out_Logs($"[错误]连接超时 TimeoutException: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Logs.Out_Logs($"[错误]网络连接中断或不可达 SocketException: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Logs.Out_Logs($"[错误]InvalidOperationException: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ismirror)
                {
                    ismirror = false;
                    await GetPreset();
                }else
                Logs.Out_Logs($"[错误]请求{fileurl}过程中发生错误：{ex.Message} "  );
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
                Logs.Out_Logs($"Error while deserializing JSON: {je.Message}");
            }

        }

        public async Task Getthank(TextBox text)
        {
            string url = "https://file.gldhn.top/file/json/thank.json";
            if(ismirror)url = Getmirror(url);
            HttpClient httpClient = new HttpClient();
            try
            {
                // 发起GET请求

                HttpResponseMessage response = await httpClient.GetAsync(url);

                // 检查响应状态是否成功
                if (response.IsSuccessStatusCode)
                {
                    // 获取响应内容的字符串形式
                    string isgitee = ismirror ? "gitee镜像" : "";
                    Logs.Out_Logs($"[提示]获取充电/发电列表成功-{isgitee}");
                    string contentString = await response.Content.ReadAsStringAsync();
                    var lists = JsonConvert.DeserializeObject<thanklist>(contentString);
                    string info = "afdian.com/@guailoudou\n|用户名|金额|\n";
                    foreach(thank item in lists.list)
                    {
                        info += $"|{item.name}|{item.num}|\n"; 
                    }
                    text.Text = info;

                }
            }
            catch (Exception ex)
            {
                Logs.Out_Logs($"[错误]请求{url}过程中发生错误：{ex.Message}");
                text.Text = "获取失败";
                if (ismirror)
                {
                    ismirror = false;
                    await Getthank(text);
                }
            }
        }
        public static string CalculateMD5Hash(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hashBytes = md5.ComputeHash(stream);
                        // Convert the byte array to hexadecimal string
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error computing MD5: {ex.Message}");
                return null;
            }
        }
        public static string Getmirror(string originalUrl)
        {
            string originalPrefix = "https://file.gldhn.top/";
            string targetPrefix = "https://gitee.com/guailoudou/urlfile/raw/main/";

            // 检查原URL是否包含需要替换的前缀
            if (originalUrl.StartsWith(originalPrefix) && ismirror)
            {
                // 替换前缀
                string convertedUrl = targetPrefix + originalUrl.Substring(originalPrefix.Length);
                return convertedUrl;
            }
            else
            {
                // 如果不匹配，则返回原URL
                return originalUrl;
            }
        }
    }
    public class thanklist
    {
        public List<thank> list { get; set; }
    }
    public class thank
    {
        public string name {  get; set; }
        public string num { get; set; }
    }
}
