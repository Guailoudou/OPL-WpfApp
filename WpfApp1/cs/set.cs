using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using static OPL_WpfApp.MainWindow_opl;

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
        public bool Auto_up { get; set; } = true;  // 自动更新s
        public bool Auto_open { get; set; } = false; //运行后自动启动
        public bool ispwarning { get; set; } = true; // 获取isp
        public bool beta { get; set; } = false;
        public List<ispinfo> ispinfos { get; set; } = new List<ispinfo>();
    }
    
    public class ispinfo
    {
        public string ip { get; set; }
        public string isp { get; set; }
    }
}
