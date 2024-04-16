using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace userdata
{
    internal class UserData
    {
        public String UUID;
        Config config;
        private static readonly Random _random = new Random();
        public UserData() {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            if (!File.Exists(absolutePath))
            {
                ResetUUID();
            }
            else
            {
                try
                {
                    string jsonCont = File.ReadAllText(absolutePath);
                    config = JsonConvert.DeserializeObject<Config>(jsonCont);
                    UUID = config.Network.Node;
                }
                catch (JsonException je)
                {
                    // 如果JSON格式不正确，记录错误并返回null
                    Console.WriteLine($"Error while deserializing JSON: {je.Message}");
                }
            }
            
        }
        public void ResetUUID()
        {
            StringBuilder sb = new StringBuilder();
            const string validChars = "0123456789abcdef";
            
            // 生成16位随机十六进制字符
            for (int i = 0; i < 16; i++)
            {
                char randomChar = validChars[_random.Next(validChars.Length)];
                sb.Append(randomChar);
            }
            UUID = sb.ToString();
            //OnPropertyChanged(nameof(UUID));
        }
        public string uuid
        {
            get { return UUID; }
        }
        

    }
}
