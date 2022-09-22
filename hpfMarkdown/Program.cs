using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace hpfMarkdown
{
    class Program
    {
        public static string RootDir
        {
            get;
            private set;
        }
        private static List<string> hpfDirs=new List<string>();
        [STAThread]
        static void Main(string[] args)
        {
            AddEnvironmentPaths();

            if (args.Length == 0)
            {
                while (true)
                {
                    Console.WriteLine("请输入指令(自动处理请输入0):");
                    var str = Console.ReadLine();
                    execute(str);
                }
            }
            else
            {
                var str = args[0];
                execute(str);
            }
        }

        private static void execute(string cmd)
        {
            var rootDir = RootDir;
            switch (cmd)
            {
                case "0":
                    BuildReportMd.Run(rootDir);

                    HpfsExport.RunOtherDirs(hpfDirs);
                    HpfsExport.Run(rootDir, true);
                    HtmlReplace.Run(rootDir);
                    Console.WriteLine("批量处理全部完成!!!\r\n");
                    break;
                case "-1":
                case "-2":
                    var apk = new Apk();
                    apk.Run(rootDir, cmd);
                    break;
            }

            ///如果是一个文件的话
            if (File.Exists(cmd))
            {
                Run(cmd);
            }
        }

        private static void Run(string updateHpfFile)
        {
            var rootDir = RootDir;
            var ext = Path.GetExtension(updateHpfFile);
            if (ext == ".hpf")
            {
                var fileName = Path.GetFileNameWithoutExtension(updateHpfFile);
                var exp_dir = string.Format("{0}/{1}/{2}/{1}", rootDir, HpfsExport.EXPORTS, fileName,
                    HpfsExport.EXPORTS);
                if (Directory.Exists(exp_dir) == false)
                {
                    HpfsExport.RunSingle(updateHpfFile, rootDir);
                }

                var md = new BuildReportChangeMd();
                md.Run(rootDir, updateHpfFile, true);

                Console.WriteLine($"complete {fileName}");
            }
            else
            {
                var dir = updateHpfFile;
                if (Directory.Exists(dir))
                {
                    var dirName = Path.GetFileNameWithoutExtension(dir);
                    switch (dirName)
                    {
                        case HpfsExport.ClientLua:
                            CrcUtil.WriteClientLua(dir);
                            break;
                        case HpfsExport.DataTable:
                            CrcUtil.WriteDataTable(dir);
                            break;
                    }

                    Console.WriteLine("complete {0}", dirName);
                }
            }
        }

        private static void AddEnvironmentPaths()
        {
            RootDir = Directory.GetCurrentDirectory();
            RootDir = RootDir.Replace("\\", "/");


            var config = RootDir + "/hpfMarkdown.cfg";
            if (File.Exists(config))
            {
                var list = File.ReadAllLines(config);
                foreach (var s in list)
                {
                    if (Directory.Exists(s))
                    {
                        hpfDirs.Add(s);
                    }
                }
            }


            CrcUtil.rootDir = RootDir;

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;


            var dll = Path.Combine(RootDir, HpfsExport.EXPORTS);
            var paths = new[] {dll};

            var path = new [] {Environment.GetEnvironmentVariable("PATH") ?? string.Empty};
            paths = path.Concat(paths).ToArray();
            var newPath = string.Join(Path.PathSeparator.ToString(), paths);

            Environment.SetEnvironmentVariable("PATH", newPath);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = new AssemblyName(args.Name);
            var path = Path.Combine(RootDir, $"exports/{assembly.Name}.dll");
            if (File.Exists(path))
            {
                return Assembly.LoadFile(path);
            }
            return null;
        }
    }

}