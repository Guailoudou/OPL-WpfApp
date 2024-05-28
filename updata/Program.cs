using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Security.Cryptography;

namespace updata
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
            string filehash = "";
            if(args.Length > 0)
            {
                filehash = args[0];
            }
            else
            {
                Console.WriteLine("不要直接打开这个，这是自动更新程序，5s后自动关闭");
                Thread.Sleep(5000);
                return;
            }
            if (CalculateMD5Hash(savePath) != filehash)
            {
                Console.WriteLine("文件校验失败，下次启动重新下载");
                File.Delete(savePath);
                return;
            }
            Console.WriteLine("文件校验完成，1s后将进行更新");
            Thread.Sleep(1000);
            
            if (File.Exists(savePath))
            {
                ExtractZipAndOverwrite(savePath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
            }
            void ExtractZipAndOverwrite(string zipPath, string extractPath)
            {
                if (File.Exists(zipPath) && Directory.Exists(extractPath))
                {
                    try
                    {
                        // 使用ZipFile.OpenRead打开zip文件，这样不会锁定文件
                        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                // 构建解压后文件的完整路径
                                string fullFilePath = Path.Combine(extractPath, entry.FullName);

                                // 确保目录存在
                                if (entry.FullName.EndsWith("/"))
                                {
                                    Directory.CreateDirectory(fullFilePath);
                                }
                                else
                                {
                                    // 如果文件已存在，则删除旧文件以准备覆盖
                                    if (File.Exists(fullFilePath))
                                    {
                                        File.Delete(fullFilePath);
                                    }

                                    // 解压文件到指定路径
                                    using (Stream inputStream = entry.Open())
                                    using (FileStream outputStream = new FileStream(fullFilePath, FileMode.CreateNew))
                                    {
                                        inputStream.CopyTo(outputStream);
                                    }
                                }
                            }
                        }
                        Console.WriteLine("解压完成，更新完毕。");
                        File.Delete(zipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解压过程中发生错误: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("ZIP文件或目标文件夹不存在。");
                }
               
            }
        }
        public static string CalculateMD5Hash(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hashBytes = md5.ComputeHash(stream);
                        // Convert the byte array to hexadecimal string
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error computing MD5: {ex.Message}");
                return null;
            }
        }
    }
}
