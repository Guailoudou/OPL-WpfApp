using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;
using WpfApp1;

namespace userdata
{
    internal class json
    {

        //uuidTextBox.Text
        Config config;
        public void newjson(UserData userData) //创建无app配置
        {
            config = new Config
            {
                Network = new Network
                {
                    Token = 11602319472897248650UL,
                    Node = userData.UUID,
                    User = "gldoffice",
                    ShareBandwidth = 100,
                    ServerHost = "api.openp2p.cn",
                    ServerPort = 27183,
                    UDPPort1 = 27182,
                    UDPPort2 = 27183,
                    TCPPort = 50448
                },
                Apps = new List<App> { },

                LogLevel = 1

            };
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
            
        }
        public void newapp(string suuid,int sport, string type,int cport=0,string appname="***")
        {
            if(cport == 0)cport = sport-1;
            App app = new App
            {
                AppName = appname,
                PeerNode = suuid,
                Whitelist = "",
                Protocol = type,
                SrcPort = cport,
                DstPort = sport,
                DstHost = "localhost",
                Enabled = 0,
                PeerUser = "",
                RelayNode = ""

            };
            config.Apps.Add(app);
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public void wejson(string ujson) //写入josn
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");

            // 创建 bin 文件夹（如果不存在）
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            using (FileStream stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(ujson);
            }
        }
       
    }


    public class App
    {
        public string AppName { get; set; }
        public string Protocol { get; set; }
        public string Whitelist { get; set; }
        public int SrcPort { get; set; }
        public string PeerNode { get; set; }
        public int DstPort { get; set; }
        public string DstHost { get; set; }
        public string PeerUser { get; set; }
        public string RelayNode { get; set; }
        public int Enabled { get; set; }
    }

    public class Network
    {
        public ulong Token { get; set; }
        public string Node { get; set; }
        public string User { get; set; }
        public int ShareBandwidth { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }
        public int UDPPort1 { get; set; }
        public int UDPPort2 { get; set; }
        public int TCPPort { get; set; }
    }

    public class Config
    {
        public Network Network { get; set; }
        public List<App> Apps { get; set; }
        public int LogLevel { get; set; }
    }
}
