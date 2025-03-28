using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Path = System.IO.Path;
using Newtonsoft.Json;
using static OPL_WpfApp.MainWindow_opl;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
namespace Tunnel
{
    public class tunconfig
    {
        private readonly string wgkeyfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin","wgkey.json");
        public tunconfig()
        {
            if (File.Exists(wgkeyfilePath)) Read();
        }
        public void Read()
        {
            try
            {
                string jsonCont = File.ReadAllText(wgkeyfilePath);
                wgkeys = JsonConvert.DeserializeObject<List<wgkey>>(jsonCont);

            }
            catch (JsonException je)
            {
                Logger.Log($"Error while deserializing JSON: {je.Message}");
            }
        }
        public List<wgkey> GetWgkeys()
        {
            return wgkeys;
        }
        public string buildconfig(bool isServer, int port = 25668,int address = 1)
        {
            if (File.Exists(wgkeyfilePath)) Read();
            Logger.Log($"Building config {address}||{port}");
            int max = wgkeys.Count;
            if (max < address)
            {
                MessageBox.Show($"id值 {address} 不行，目前最大{max}，且一定不可和其他人重复");
                return "err";
            }
            string Interface = $"[Interface]\r\nPrivateKey = {wgkeys[address-1].PrivateKey}\r\nAddress = {wgkeys[address-1].Address}\r\nDNS = 223.5.5.5,8.8.8.8\r\nPostUp = powershell -Command \"Set-NetIPInterface -InterfaceAlias 'WireGuard' -MulticastForwarding Enabled\"\r\n";
            string Peers = "";
            int i = 0;
            if (isServer)
            {
                Interface += $"ListenPort = {port}\r\n";
                foreach (var wgkey in wgkeys) { 
                    if(i!=0)
                        Peers += $"[Peer]\r\nPublicKey = {wgkey.PublicKey}\r\nAllowedIPs = 10.0.23.{i+1}/32, 224.0.0.0/8\r\nPersistentKeepalive = 25\r\n";
                    i++;
                }
            }
            else
            {
                Peers = $"[Peer]\r\nPublicKey = {wgkeys[0].PublicKey}\r\nAllowedIPs = 10.0.23.0/24, 224.0.0.0/8\r\nEndpoint = 127.0.0.1:{port}\r\nPersistentKeepalive = 25\r\n";
            }
            return $"{Interface}\r\n{Peers}";
        }
        private List<wgkey> wgkeys;
        public class wgkey
        {
            public string PrivateKey;
            public string PublicKey;
            public string Address;
            public string toString() => $"PrivateKey: {PrivateKey}\r\nPublicKey: {PublicKey}\r\nAddress: {Address}";
        }
    }
}
