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
            Console.WriteLine("*************************");
            Console.WriteLine("");
            Console.WriteLine("Openp2p Launcher 更新程序");
            Console.WriteLine("");
            Console.WriteLine("*************************");
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "nvb.zip");
            string filehash = "";
            string oldopl = "";
            if(args.Length > 0)
            {
                filehash = args[0];
                if (args.Length > 1)
                {
                    string filepath = "";
                    foreach(string arg in args)
                    {
                        if (arg != filehash)
                        {
                            if(filepath == "")
                                filepath += arg;
                            else
                                filepath += " "+arg;
                        }
                    }
                    oldopl = Path.GetFileName(filepath);
                    Console.WriteLine("文件路径：" + filepath);
                    
                    
                }
                Console.WriteLine("文件哈希：" + filehash);
                Console.WriteLine("文件名：" + oldopl);
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
            Console.WriteLine("文件校验完成，1s后将进行更新，请勿关闭本窗口");
            Thread.Sleep(1000);
            //DeleteAllDllFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
            if (File.Exists(savePath))
            {
                ExtractZipAndOverwrite(savePath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
            }
            void ExtractZipAndOverwrite(string zipPath, string extractPath)
            {
                if (File.Exists(zipPath) && Directory.Exists(extractPath))
                {
                    string tempDir = Path.Combine(extractPath, "opl_update_temp");
                    try
                    {
                        // 先清理可能残留的临时目录
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);

                        // 第一步：解压到临时目录
                        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string fullFilePath = Path.Combine(tempDir, entry.FullName);

                                if (entry.FullName.EndsWith("/"))
                                {
                                    Directory.CreateDirectory(fullFilePath);
                                }
                                else
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));
                                    using (Stream inputStream = entry.Open())
                                    using (FileStream outputStream = new FileStream(fullFilePath, FileMode.Create))
                                    {
                                        inputStream.CopyTo(outputStream);
                                    }
                                }
                            }
                        }

                        // 第二步：全部解压成功后才覆盖到目标目录
                        foreach (string file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
                        {
                            string relativePath = file.Substring(tempDir.Length + 1);
                            string destPath = Path.Combine(extractPath, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                            if (File.Exists(destPath))
                                File.Delete(destPath);
                            File.Move(file, destPath);
                        }

                        // 清理临时目录
                        Directory.Delete(tempDir, true);

                        if (oldopl != "OPL_WpfApp.exe" && oldopl != "")
                        {
                            File.Delete(Path.Combine(extractPath, oldopl));
                            File.Move(Path.Combine(extractPath, "OPL_WpfApp.exe"), Path.Combine(extractPath, oldopl));
                        }
                        Console.WriteLine("解压完成，更新完毕。已可关闭本窗口，再次启动即为最新版，更新内容请关注关于-日志");
                        File.Delete(zipPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解压过程中发生错误，请尝试手动移动opl_update_temp中的文件替换主程序文件: {ex.Message}");
                        Thread.Sleep(5000);
                        // 清理临时目录
                        //try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
                    }
                }
                else
                {
                    Console.WriteLine("ZIP文件或目标文件夹不存在。");
                }
               
            }
            Console.WriteLine($"该窗口已可关闭，或等待5s后自动关闭");
            Thread.Sleep(5000);
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
