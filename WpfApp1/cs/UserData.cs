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
        public String UID;
        json json;
        private static readonly Random _random = new Random();
        public UserData() {
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "config.json");
            if (!File.Exists(absolutePath))
            {
                ResetUID();
            }
            else
            {
                json = new json();
                UID = json.config.Network.Node;
                if(UID == "auto")
                {
                    ResetUID();
                    json.config.Network.Node = UID;
                    json.Save();
                }
            }

        }
        public void ResetUID()
        {
            StringBuilder sb = new StringBuilder();
            const string validChars = "0123456789abcdef";
            
            // 生成16位随机十六进制字符
            for (int i = 0; i < 16; i++)
            {
                char randomChar = validChars[_random.Next(validChars.Length)];
                sb.Append(randomChar);
            }
            UID = sb.ToString();
            //OnPropertyChanged(nameof(UID));
        }
        public string uid
        {
            get { return UID; }
        }
        

    }
}
