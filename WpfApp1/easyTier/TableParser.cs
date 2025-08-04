using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPL_WpfApp.easyTier
{
    public static class TableParser
    {
        /// <summary>
        /// 解析 ASCII 表格格式的字符串，返回 NetworkNode 对象数组
        /// </summary>
        /// <param name="input">表格文本</param>
        /// <returns>NetworkNode 数组</returns>
        public static NetworkNode[] ParseTable(string input)
        {
            var nodes = new List<NetworkNode>();

            // 按行分割
            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                             .Select(x => x.Trim())
                             .Where(x => !string.IsNullOrEmpty(x) && x.StartsWith("│"))
                             .ToArray();

            if (lines.Length == 0) throw new ArgumentException("输入为空或格式不正确");

            // 提取表头（第一行）
            string headerLine = lines[0];
            string[] headers = SplitLine(headerLine);

            // 查找列索引（防止列顺序变化）
            int ipv4Idx = Array.IndexOf(headers, "ipv4");
            int hostnameIdx = Array.IndexOf(headers, "hostname");
            int costIdx = Array.IndexOf(headers, "cost");
            int latMsIdx = Array.IndexOf(headers, "lat_ms");
            int lossRateIdx = Array.IndexOf(headers, "loss_rate");
            int rxBytesIdx = Array.IndexOf(headers, "rx_bytes");
            int txBytesIdx = Array.IndexOf(headers, "tx_bytes");
            int tunnelProtoIdx = Array.IndexOf(headers, "tunnel_proto");
            int natTypeIdx = Array.IndexOf(headers, "nat_type");
            int idIdx = Array.IndexOf(headers, "id");
            int versionIdx = Array.IndexOf(headers, "version");

            // 验证关键列是否存在
            if (ipv4Idx == -1 || idIdx == -1)
                throw new InvalidOperationException("表头缺少必要字段");

            // 解析数据行（跳过第一行表头）
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = SplitLine(lines[i]);

                // 防止越界
                if (cells.Length < headers.Length)
                {
                    Array.Resize(ref cells, headers.Length); // 补齐缺失列
                }

                var node = new NetworkNode
                {
                    Ipv4 = SafeGet(cells, ipv4Idx),
                    Hostname = SafeGet(cells, hostnameIdx),
                    Cost = SafeGet(cells, costIdx),
                    LatMs = SafeGet(cells, latMsIdx),
                    LossRate = SafeGet(cells, lossRateIdx),
                    RxBytes = SafeGet(cells, rxBytesIdx),
                    TxBytes = SafeGet(cells, txBytesIdx),
                    TunnelProto = SafeGet(cells, tunnelProtoIdx),
                    NatType = SafeGet(cells, natTypeIdx),
                    Id = SafeGet(cells, idIdx),
                    Version = SafeGet(cells, versionIdx)
                };

                nodes.Add(node);
            }

            return nodes.ToArray();
        }
        private static string[] SplitLine(string line)
        {
            return line
                .Split('│')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0) // 去除首尾空项
                .ToArray();
        }

        // 安全获取，越界返回空字符串
        private static string SafeGet(string[] array, int index)
        {
            return index >= 0 && index < array.Length ? array[index] : "";
        }
        //private static string ExtractCell(string line, int start, int end)
        //{
        //    if (start >= line.Length) return "";
        //    int actualEnd = Math.Min(end, line.Length);
        //    return line.Substring(start, actualEnd - start).Trim();
        //}
        
    }
    public class NetworkNode
    {
        public string Ipv4 { get; set; }
        public string Hostname { get; set; }
        public string Cost { get; set; }
        public string LatMs { get; set; }
        public string LossRate { get; set; }
        public string RxBytes { get; set; }
        public string TxBytes { get; set; }
        public string TunnelProto { get; set; }
        public string NatType { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }

        public override string ToString()
        {
            return $"IPv4: {Ipv4}, Hostname: {Hostname}, Cost: {Cost}, Latency: {LatMs} ms, NAT: {NatType}, ID: {Id}";
        }
    }
}
