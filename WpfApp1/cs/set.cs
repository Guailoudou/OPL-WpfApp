using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using OPL_WpfApp.Utils;

namespace userdata 
{ 
    internal class set
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "set.json");
        public settings settings;
        public set()
        {
            settings = new settings();
            if (File.Exists(filePath)) Read();
            else Write();
            settings.Instance = settings; // 供全局只读访问（日志/性能模块等）
        }
        public void Read()
        {
            try
            {
                string jsonCont = File.ReadAllText(filePath);
                settings = JsonConvert.DeserializeObject<settings>(jsonCont);

            }
            catch (JsonException je)
            {
                Logger.Log($"Error while deserializing JSON: {je.Message}");
            }
        }
        public void Write()
        {
            string text = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(text);
            }
        }
        public static Color ParseColor(string colorString)
        {
            // 确保字符串是以 '#' 开头的
            if (!colorString.StartsWith("#"))
            {
                Logger.Log("The color string must start with '#'.");
            }

            // 去掉第一个字符（'#'）
            string hexValue = colorString.Substring(1);

            // 如果没有 alpha 通道，我们添加一个默认的 FF (255) 表示不透明
            if (hexValue.Length == 6)
            {
                hexValue = "FF" + hexValue; // 添加默认的 alpha 通道
            }

            // 将十六进制字符串转换为整数
            int colorInt = Convert.ToInt32(hexValue, 16);

            // 提取各个颜色通道的值
            byte a = (byte)((colorInt >> 24) & 0xFF);
            byte r = (byte)((colorInt >> 16) & 0xFF);
            byte g = (byte)((colorInt >> 8) & 0xFF);
            byte b = (byte)(colorInt & 0xFF);

            // 创建并返回 Color 对象
            return Color.FromArgb(a, r, g, b);
        }
    }
    public class settings
    {
        public string Color { get; set; } // 颜色
        public string Theme { get; set; } // 主题("Light" 或 "Dark")
        public string csproduct { get; set; } //BIOSUID
        public bool Auto_upop { get; set; } = true; // 自动更新openp2p
        public bool Auto_upet { get; set; } = true; // 自动更新EasyTier
        public bool Auto_up { get; set; } = true;  // 自动更新s
        public bool Auto_open { get; set; } = false; //运行后自动启动
        public bool ispwarning { get; set; } = true; // 获取isp
        public bool minimize { get; set; } = true;  //最小化到托盘
        public bool qusminimize { get; set; } = true;  //是否询问最小化到托盘
        public bool CleanLogOnExit { get; set; } = false; // 退出时清理日志缓存
        public bool beta { get; set; } = false;
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> EasyTierPeers { get; set; } = new List<string> { "tcp://p.gldhn.top:11010" };
        public List<ispinfo> ispinfos { get; set; } = new List<ispinfo>();
        public string LastNoticeTime { get; set; } = ""; // 上次已读公告的时间

        // ==================== 外观与交互 ====================
        public bool Animations { get; set; } = true;            // 界面动画效果总开关
        public string Backdrop { get; set; } = "Mica";          // 窗口背景材质：Mica / Acrylic / None
        public double UiFontSize { get; set; } = 14;            // 界面字号
        public bool CompactNav { get; set; } = false;           // 紧凑导航栏（仅图标）
        public bool AutoHideLogOnBackground { get; set; } = true; // 后台时暂缓日志界面刷新

        // ==================== 性能与资源 ====================
        public string PerfMode { get; set; } = "Balanced";      // 运行模式：HighPerformance / Balanced / PowerSave
        public bool TrimOnMinimize { get; set; } = true;        // 最小化时回收内存
        public bool AggressiveTrim { get; set; } = true;        // 隐藏到托盘后深度回收工作集
        public int EtPollSeconds { get; set; } = 5;             // EasyTier 节点状态刷新间隔（秒）
        public int TrafficPollSeconds { get; set; } = 1;        // 虚拟网卡流量刷新间隔（秒）
        public bool KeepAwake { get; set; } = false;            // 联机期间阻止系统休眠

        // ==================== 日志与缓存 ====================
        public bool FileLogEnabled { get; set; } = true;        // 写日志文件开关
        public int UiLogMaxLines { get; set; } = 300;           // 界面日志最大保留行数
        public bool TimestampInLog { get; set; } = true;        // 日志显示时间戳

        // ==================== 通知与提示 ====================
        public bool NoticePopup { get; set; } = true;           // 新公告弹窗提醒
        public bool TrayBalloonTip { get; set; } = true;        // 托盘气泡通知（服务状态变化）
        public bool MinimizeOnStartup { get; set; } = false;    // 启动后最小化到托盘
        public bool ToastOnTunnelConnected { get; set; } = false; // 隧道连成时弹出提醒

        // ==================== 连接细节 ====================
        public string DefaultProtocol { get; set; } = "tcp";    // 新建隧道默认协议
        public bool AutoCopyLinkCode { get; set; } = true;      // 创建组网后自动复制联机码
        public bool HumanReadableTraffic { get; set; } = true;  // 流量以 KB/MB 人性化显示
        public bool ConfirmBeforeCloseAll { get; set; } = true; // 关闭所有隧道前二次确认
        public string EasyTierExtraArgs { get; set; } = "";     // EasyTier 高级启动参数（追加）

        /// <summary>全局设置快照（只读用途），未初始化时返回默认值实例</summary>
        [JsonIgnore]
        public static settings Instance { get; internal set; }
        [JsonIgnore]
        public static settings Safe => Instance ?? new settings();

        /// <summary>运行模式对应的轮询间隔倍率</summary>
        [JsonIgnore]
        public double PollFactor => PerfMode == "HighPerformance" ? 0.5 : PerfMode == "PowerSave" ? 3 : 1;
    }
    
    public class ispinfo
    {
        public string ip { get; set; }
        public string isp { get; set; }
    }
}
