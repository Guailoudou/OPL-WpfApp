﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using static OPL_WpfApp.MainWindow;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
namespace userdata
{
    internal class json
    {

        int Ologv = 2; //openp2p日志等级
        public Config config;
        List<int> oindex = new List<int>();
        public json()
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            if (!File.Exists(absolutePath))
            {
                UserData userData = new UserData();
                userData.ResetUUID();
                newjson(userData);
            }
            else
            {
                getjosn();
            }
        }
        public void newjson(UserData userData) //创建无app配置
        {
            Logger.Log("[执行]创建新的配置-UID:" + userData.UUID);
            config = new Config
            {
                Network = new Network
                {
                    Token = 11602319472897248650UL,
                    Node = userData.UUID,
                    User = "gldoffice",
                    ShareBandwidth = 10,
                    ServerHost = "api.openp2p.cn",
                    ServerPort = 27183,
                    UDPPort1 = 27182,
                    UDPPort2 = 27183,
                    TCPPort = 50448
                },
                Apps = new List<App> { },

                LogLevel = Ologv

            };
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
            
        }
        public void Alloff()
        {
            foreach (App app in config.Apps)
            {
                app.Enabled = 0;
            }
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public void Add1link(string type, string uid, int port,int cport)  
        {
            
            int index = Finduid(uid,type);
            if (index != -1)
            {
                config.Apps[index].DstPort = port;
                if(cport!=0)config.Apps[index].SrcPort = cport;
                config.Apps[index].Enabled = 1;
                oindex.Add(index);
                string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
                wejson(ujson);
            }
            else
            {
                newapp(uid,port,type,cport);
                index = config.Apps.Count-1 ;
                oindex.Add(index);
            }
        }
        public int Finduid(string uid,string type)
        {
            int index = 0;
            foreach (App app in config.Apps)
            {
                if (app.PeerNode==uid && app.Protocol==type && !oindex.Contains(index))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        public void Setshare(int n)
        {
            config.Network.ShareBandwidth = n;
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public bool newapp(string suuid,int sport, string type,int cport=0,string appname="自定义")
        {
            if (suuid == config.Network.Node)
            {
                Logger.Log("[错误]自己连自己？");
                MessageBox.Show("不能自己连自己啊！！这无异于试图左脚踩右脚升天！！", "错误");
                return false;
            }
            if (cport == 0) cport = sport;
            Logger.Log("[执行]创建新的隧道"+suuid+":"+sport+"--"+type+">>"+cport);
            int enabled = 1;

            if (config.Apps != null)
                foreach (App apps in config.Apps)
                {
                    if (apps.Enabled == 1 && cport == apps.SrcPort && type == apps.Protocol)
                    {
                        apps.Enabled = 0;
                    }
                }

            App app = new App
            {
                AppName = appname,
                PeerNode = suuid,
                Whitelist = "",
                Protocol = type,
                SrcPort = cport,
                DstPort = sport,
                DstHost = "localhost",
                Enabled = enabled,
                PeerUser = "",
                RelayNode = ""

            };
            if (config.Apps != null)
                config.Apps.Add(app);
            else
            {

                config.Apps = new List<App> { 
                    app
                };
            }
            config.LogLevel = Ologv;
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
            return true;
        }
        public void ReSetToken()
        {
            getjosn();
            config.Network.Token = 11602319472897248650UL;
            config.LogLevel = Ologv;
            config.Network.User = "gldoffice";
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
        public void del(int index)
        {
            Logger.Log("[执行]删除隧道 序号:"+index);
            getjosn();
            config.Apps.RemoveAt(index);
            config.LogLevel = Ologv;
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public void onapp(int index) //开启app
        {
            getjosn();
            
            foreach (App app in config.Apps)
            {
                if (app.Enabled == 1 && config.Apps[index].SrcPort==app.SrcPort && config.Apps[index].Protocol == app.Protocol)
                {
                    Logger.Log("[错误]无法同时开启2个本地端口相同的隧道");
                    MessageBox.Show("无法同时开启2个本地端口相同的隧道", "警告");
                    return;
                }
            }
            config.Apps[index].Enabled = 1;
            config.LogLevel = Ologv;
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public void offapp(int index) //关闭app
        {
            getjosn();
            config.Apps[index].Enabled = 0;
            config.LogLevel = Ologv;
            string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
            wejson(ujson);
        }
        public void getjosn() //读取配置
        {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            try
            {
                string jsonCont = File.ReadAllText(absolutePath);
                config = JsonConvert.DeserializeObject<Config>(jsonCont);
                if (config.Network.ShareBandwidth == 100) { 
                    config.Network.ShareBandwidth = 10;
                    string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
                    wejson(ujson);
                }
                if(config.LogLevel != Ologv) 
                {
                    config.LogLevel = Ologv;
                    string ujson = JsonConvert.SerializeObject(config, Formatting.Indented);
                    wejson(ujson);
                }

            }
            catch (JsonException je)
            {
                // 如果JSON格式不正确，记录错误并返回null
                Logger.Log($"Error while deserializing JSON: {je.Message}");
            }

        }
       
    }

    public static class ConnectionParser
    {
        public static List<ConnectionInfo> ParseConnections(string input)
        {
            var connections = new List<ConnectionInfo>();
            if (string.IsNullOrEmpty(input)) return connections;

            // 分割字符串中的多个连接信息
            var parts = input.Split(';');
            foreach (var part in parts)
            {
                var components = part.Split(':');
                if (components.Length!=2 &&components.Length != 3 && components.Length != 4)
                {
                    throw new ArgumentException("连接格式无效", nameof(input));
                }
                if(components.Length == 2)
                {
                    if (!int.TryParse(components[1], out int tmport))
                    {
                        throw new ArgumentException("端口不是有效整数", nameof(input));
                    }
                    if (tmport <= 0 || tmport > 65535)
                    {
                        throw new ArgumentException("端口不在正确范围(0~65535)", nameof(input));
                    }
                    connections.Add(new ConnectionInfo
                    {
                        Protocol = "1",
                        UID = components[0],
                        Port = tmport,
                        CPort = 0
                    });
                    break;
                }
                if (components[0] != "1" && components[0] != "2")
                {
                    throw new ArgumentException("协议无效", nameof(input));
                }
                if (!int.TryParse(components[2], out int port))
                {
                    throw new ArgumentException("端口不是有效整数", nameof(input));
                }
                int cport = 0;
                if (components.Length == 4) {
                    if (!int.TryParse(components[3], out int tport))
                    {
                        throw new ArgumentException("端口不是有效整数", nameof(input));
                    }
                    cport = tport;
                }
                if (port <= 0 || port > 65535)
                {
                    throw new ArgumentException("端口不在正确范围(0~65535)", nameof(input));
                }
                var connection = new ConnectionInfo
                {
                    Protocol = components[0],
                    UID = components[1],
                    Port = port,
                    CPort = cport
                };

                connections.Add(connection);
            }

            return connections;
        }
    }

    public class App
    {
        public string AppName { get; set; }
        public string Protocol { get; set; } //隧道类型
        public string Whitelist { get; set; }
        public int SrcPort { get; set; } //本地端口
        public string PeerNode { get; set; } //被连uuid
        public int DstPort { get; set; } //远程端口
        public string DstHost { get; set; } //远程ip
        public string PeerUser { get; set; }
        public string RelayNode { get; set; }
        public int Enabled { get; set; } //开启？
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

    public class preset
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public List<PrTunnel> tunnel { get; set; }
    }

    public class PrTunnel
    {
        public int Sport { get; set; }
        public int CPort { get; set; }
        public string type { get; set;}
    }

    public class Presets
    {
        public List<preset> presets { get; set; }
        public int version { get; set; }
        public string uplog { get; set; }
        public string upurl { get; set; }
        public string uphash { get; set; }
        public string opurl { get; set; }
        public string ophash { get; set; }
    }

    public class ConnectionInfo
    {
        public string Protocol { get; set; }
        public string UID { get; set; }
        public int Port { get; set; }
        public int CPort { get; set; }
        public override string ToString()
        {
            return $"{Protocol}:{UID}:{Port}";
        }
    }
}
