using hpfMarkdown.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace hpfMarkdown
{
    public class HpfExportVO
    {
        private string _fileName;
        private string _filePath;

        public Action<HpfExportVO> onComplete;

        public string filePath
        {
            set
            {
                _filePath = value;
                _fileName = Path.GetFileNameWithoutExtension(_filePath);
            }
            get { return _filePath; }
        }

        public string savePath;

        public void export()
        {
            var result = HpfSys.ExportHpfFiles(_filePath, 0L, savePath, callback, false);
        }

        private void callback(int a, int b, string msg)
        {
            if (a != b)
                Console.WriteLine("{2}: \t{0}/{1}", a, b, _fileName);
            else
            {
                onComplete?.Invoke(this);
            }
        }
    }

    public class HpfsExport
    {
        public const string EXPORTS = "exports";
        public const string ClientLua = "ClientLua";
        public const string DataTable = "DataTable";

        public static void Run(string rootDir,bool isRoot=false)
        {
            var files = Directory.GetFiles(rootDir, "*.hpf", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                RunSingle(file);
            }

            if (isRoot)
            {
                foreach (var file in files)
                {
                    var md = new BuildReportChangeMd();
                    md.Run(rootDir, file);
                }

                var exports = Path.Combine(rootDir, EXPORTS);
                if (Directory.Exists(exports) == false)
                {
                    return;
                }

                var dic = new Dictionary<string, List<string>>();
                files = Directory.GetFiles(exports, "*.md", SearchOption.TopDirectoryOnly);
               
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileNameSplit=fileName.Split('_');
                    var len = fileNameSplit.Length;
                    if (len < 3)
                    {
                        continue;
                    }

                    var platform = fileNameSplit[len - 1];
                    List<string> list;
                    if (dic.TryGetValue(platform, out list) == false)
                    {
                        dic[platform] = list = new List<string>();
                    }

                    list.Add(file);
                }

                var sb = new StringBuilder();
                sb.AppendLine(MD.Link("->版本表  ", "index.md"));
                var line = "";
                foreach (var key in dic.Keys)
                {
                    var list = dic[key];
                    if (list.Count > 0)
                    {
                        list.Sort();
                        list.Reverse();
                        sb.AppendLine(MD.H1(key));
                        foreach (var file in list)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            line = MD.Link(fileName, fileName + ".md");
                            var fs = File.OpenRead(file);
                            sb.Append(MD.Li(line));
                            using (var reader=new StreamReader(fs))
                            {
                                line=reader.ReadLine();//返回
                                line = reader.ReadLine();
                                sb.AppendFormat("\t\t{0}",line);
                            }

                            sb.Append("\n");
                        }
                        sb.Append("\n");
                    }
                }

                var path = exports + "/arts.md";

                Console.WriteLine("Complete arts.md");
                File.WriteAllText(path, sb.ToString());
            }
        }


        public static void RunOtherDirs(List<string> hpfDirs)
        {
            var list=new List<string>();
            foreach (var hpfDir in hpfDirs)
            {
                var files = Directory.GetFiles(hpfDir, "*.hpf", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.Contains("inpakPatch_"))
                    {
                        continue;
                    }

                    list.Add(file);
                }
            }

            var rootDir = Program.RootDir;
            foreach (var file in list)
            {
                RunSingle(file,rootDir);

                var md = new BuildReportChangeMd();
                md.Run(rootDir, file);
            }
        }

        public static void RunSingle(string file,string saveDir="")
        {
            //eg Android_updatepatch_[0818_192609_14.566040_14.567332]patch_14.566040_14.567332.hpf;
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrEmpty(saveDir))
            {
                saveDir = Path.GetDirectoryName(file);
            }

            var exportDir = string.Format("{0}/{1}/{2}", saveDir, EXPORTS, fileName);
            if (Directory.Exists(exportDir) == true)
            {
                return;
            }

            Directory.CreateDirectory(exportDir);

            var hpfex = new HpfExportVO();
            hpfex.filePath = file;
            hpfex.savePath = exportDir;
            hpfex.onComplete = itemOnComplete;
            hpfex.export();
        }

        private static void itemOnComplete(HpfExportVO vo)
        {
            Run(vo.savePath);
        }
    }
}