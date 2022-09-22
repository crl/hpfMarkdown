using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using hpfMarkdown.utils;

namespace hpfMarkdown
{
    public class CrcUtil
    {
        private const string TMP = "{0}/{1}/{2}.txt";
        public static string rootDir;
        public static void WriteDataTable(string dir)
        {
            var files = Directory.GetFiles(dir, "*.bytes", SearchOption.AllDirectories);

            var sb = new StringBuilder();
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var crc = Crc32.Get(fileName).ToString();
                var line = string.Format("{0}={1}", fileName, crc);
                sb.AppendLine(line);

                Console.WriteLine(line);
            }

            var savePath = string.Format(TMP, rootDir, HpfsExport.EXPORTS, HpfsExport.DataTable);
            File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
        }

        public static void WriteClientLua(string dir)
        {
            var head = "ClientLua";
            var end = ".bytes";
            var sb = new StringBuilder();
            var files = Directory.GetFiles(dir, "*.bytes", SearchOption.AllDirectories);
            foreach (var path in files)
            {
                var file = path.Replace("\\","/");

                file = file.Substring(file.IndexOf(head));
                file = file.Replace(head + "/", "");
                file = file.Replace(end, "");
                file = file.Replace("/", ".");
                var crc = Crc32.Get(file).ToString();

                var line = string.Format("{0}={1}", file, crc);
                sb.AppendLine(line);

                Console.WriteLine(line);
            }

            var savePath =string.Format(TMP,rootDir,HpfsExport.EXPORTS,HpfsExport.ClientLua);
            File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
        }

        private static Dictionary<string, string> CrcMaping;
        public static bool TryGetValue(string key, out string rawName)
        {
            if (CrcMaping == null)
            {
                CrcMaping = new Dictionary<string, string>();
                var list = new string[] {HpfsExport.DataTable, HpfsExport.ClientLua};
                foreach (var item in list)
                {
                    var savePath = string.Format(TMP, rootDir, HpfsExport.EXPORTS, item);
                    if (File.Exists(savePath))
                    {
                        var lines = File.ReadAllLines(savePath);

                        foreach (var line in lines)
                        {
                            var splis = line.Split('=');
                            if (splis.Length < 2)
                            {
                                continue;
                            }

                            CrcMaping[splis[1]] = splis[0];
                        }
                    }
                }
            }

            return CrcMaping.TryGetValue(key, out rawName);
        }
    }
}