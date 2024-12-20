using Serilog;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace OplWpf.Models;

public class IpInfo //https://uapis.cn/api/ipinfo
{
    public int code { get; set; }
    public string ip { get; set; }
    public string beginip { get; set; }
    public string endip { get; set; }
    public string region { get; set; }
    public string asn { get; set; }
    public string isp { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string LLC { get; set; }
}

public class Net
{
    private static readonly int pvn = 46;//协议版本号

    public static int Pvn => pvn;

    public static async Task GetIsp(string ip)
    {
        string url = "https://uapis.cn/api/ipinfo?ip=" + ip;
        string isp = "";
        var httpClient = new HttpClient();
        try
        {
            // 发起GET请求

            HttpResponseMessage response = await httpClient.GetAsync(url);

            // 检查响应状态是否成功
            if (response.IsSuccessStatusCode)
            {
                string contentString = await response.Content.ReadAsStringAsync();
                var ipInfo = JsonSerializer.Deserialize<IpInfo>(contentString);
                if (ipInfo?.code == 200)
                {
                    isp = ipInfo.isp;
                    Log.Information("经检测你的网络运营商为：{isp} --数据由Uapi提供", isp);
                    if (isp != "电信" && isp != "联通" && isp != "移动")
                    {
                        if (ConfigManager.Instance.Setting.IspWarning)
                        {
                            MessageBox.Show($"检测到你的网络运营商为非电信、联通、移动，你的运营商为{isp}，可能为二级运营商，二级运营商连接或被连接可能受阻，或长时间无法成功连接。如果你不在国内或为其他一级运营商（国内仅这3家为一级运营商），你可以在设置关闭运营商检测提醒。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    Log.Error("获取{ip}的运营商信息失败", isp);
                }


            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "请求{url}过程中发生错误", url);
        }
    }
}
