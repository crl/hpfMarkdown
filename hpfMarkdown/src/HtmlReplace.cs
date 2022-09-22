using System;
using System.IO;

namespace hpfMarkdown
{
    public class HtmlReplace
    {
        private const string key = "href=\"chrome-extension://dnlmgcobbhafacmajffabmcopmfkieio/\"";
        public static void Run(string rootDir)
        {
            var dir = rootDir + "/" + HpfsExport.EXPORTS;
            var files = Directory.GetFiles(dir, "*.html", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                RunSingle(file);
            }

        }

        private static void RunSingle(string file)
        {
            var isChange = false;
            var lines = File.ReadAllLines(file);
            for (int i = 0,len=lines.Length; i < len; i++)
            {
                var line = lines[i];
                var index=line.IndexOf(key);
                if (index != -1)
                {
                    lines[i] = line.Replace(key, "");
                    isChange = true;
                    break;
                }
            }

            if (isChange)
            {
                File.WriteAllLines(file, lines);
            }
        }
    }
}