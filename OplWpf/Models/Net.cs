using Serilog;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace OplWpf.Models;

public class IpInfo //https://uapis.cn/api/ipinfo
{
    public int Code { get; set; }
    public required string Ip { get; set; }
    public required string BeginIp { get; set; }
    public required string EndIp { get; set; }
    public required string Region { get; set; }
    public required string Asn { get; set; }
    public required string Isp { get; set; }
    public required string Latitude { get; set; }
    public required string Longitude { get; set; }
    public required string LLC { get; set; }
}

public class Thank
{
    public required string Name { get; set; }
    public required string Num { get; set; }
}

public class ThankList
{
    public required IReadOnlyList<Thank> List { get; set; }
}

public class Net
{
    private static readonly int pvn = 46;//协议版本号
    private static IReadOnlyList<string> CommonIsp = ["电信", "联通", "移动"];

    public static int Pvn => pvn;

    public static async Task GetIsp(string ip)
    {
        var url = "https://uapis.cn/api/ipinfo?ip=" + ip;
        var httpClient = new HttpClient();
        try
        {
            // 发起GET请求
            var response = await httpClient.GetAsync(url);

            // 检查响应状态是否成功
            if (response.IsSuccessStatusCode)
            {
                var ipInfo = await response.Content.ReadFromJsonAsync<IpInfo>();
                //string contentString = await response.Content.ReadAsStringAsync();
                //var ipInfo = JsonSerializer.Deserialize<IpInfo>(contentString, JsonSerializerOptions.Web);
                if (ipInfo?.Code == 200)
                {
                    var isp = ipInfo.Isp;
                    Log.Information("经检测你的网络运营商为：{isp} --数据由Uapi提供", isp);
                    if (!CommonIsp.Contains(isp) && ConfigManager.Instance.Setting.IspWarning)
                    {
                        MessageBox.Show(
                            $"检测到你的网络运营商为非电信、联通、移动，你的运营商为{isp}，可能为二级运营商，二级运营商连接或被连接可能受阻，或长时间无法成功连接。如果你不在国内或为其他一级运营商（国内仅这3家为一级运营商），你可以在设置关闭运营商检测提醒。",
                            "提示",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                }
                else
                {
                    Log.Error("获取{ip}的运营商信息失败", ip);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "请求{url}过程中发生错误", url);
        }
    }

    public static async Task<string> GetDaySayAsync()
    {
        var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync("https://uapis.cn/api/say");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "获取失败";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "获取每日一句失败");
            return "获取失败";
        }
    }

    public static async Task<IReadOnlyList<Thank>> GetThankListAsync()
    {
        string url = "https://file.gldhn.top/file/json/thank.json";
        var httpClient = new HttpClient();
        try
        {
            // 发起GET请求
            var response = await httpClient.GetAsync(url);

            // 检查响应状态是否成功
            if (response.IsSuccessStatusCode)
            {
                // 获取响应内容的字符串形式
                //string isgitee = ismirror ? "gitee镜像" : "";

                Log.Information("获取充电/发电列表成功");
                //string contentString = await response.Content.ReadAsStringAsync();
                //var lists = JsonSerializer.Deserialize<ThankList>(contentString, JsonSerializerOptions.Web)
                var lists = await response.Content.ReadFromJsonAsync<ThankList>()
                    ?? throw new InvalidCastException("数据格式不正确");
                return lists.List;
            }
            return [];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "请求{url}过程中发生错误", url);
            return [];
        }
    }
}
