using hpfMarkdown.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace hpfMarkdown
{
    public class BuildReportChangeMd
    {
        private const string Ver_Prefix = "1.0.";
        private const string ABKey = "GamePatch";
        Dictionary<string, ReportAssetBundle> fromABS = null;
        Dictionary<string, ReportAssetBundle> toABS = null;
        HashSet<string> updateCrcNames = new HashSet<string>();

        Dictionary<string,string> allsURL=new Dictionary<string, string>();
        private FormatABVO formatABVO;
        internal void Run(string rootDir, string hpfFilePath,bool autoOpen=false)
        {
            var updateFileName = Path.GetFileNameWithoutExtension(hpfFilePath);
            var verSplits = updateFileName.Split('_');
            var len = verSplits.Length;

            if (len < 2)
            {
                Console.WriteLine("error:不符合版本规则:{0}", updateFileName);
                return;
            }

            var platform = verSplits[0];
            var fromS = verSplits[len - 2];
            var toS = verSplits[len - 1];
            var from = Ver_Prefix +fromS;
            var to = Ver_Prefix + toS;
            var timeStr = "";
            var index = updateFileName.IndexOf('[');
            len = updateFileName.IndexOf(']') - index;
            if (len > 0)
            {
                timeStr = updateFileName.Substring(index + 1, len - 1);
                timeStr = timeStr.Split('_')[0];
            }

            var mdFile = rootDir + $"/exports/{timeStr}_{fromS}-{toS}_{platform}.md";
            if (File.Exists(mdFile))
            {
                Console.WriteLine("已存在:{0}", mdFile);
                if (autoOpen)
                {
                    Process.Start(mdFile);
                }

                return;
            }

            var dirs = new List<string>();
            dirs.Add(string.Format("{0}/{1}/{2}/{1}", rootDir, HpfsExport.EXPORTS, updateFileName));
            dirs.Add(string.Format("{0}/{1}/{2}/{3}", rootDir, HpfsExport.EXPORTS, updateFileName,"GeneratedSoundBanks"));

            var path =BuildReportMd.GetIndexMdPath();
            if (File.Exists(path)==false)
            {
                BuildReportMd.Run(rootDir);
            }
            var lines = File.ReadAllLines(path);
            var url = "";
            var key = "";
            var currentPlatform = "";
            foreach (var line in lines)
            {
                if (line.StartsWith("####  "))
                {
                    var splts = line.Split('_');
                    currentPlatform = splts[splts.Length - 1];
                }

                if (line.StartsWith("1. [1.0") == false)
                {
                    continue;
                }

                index = line.IndexOf("[");
                len = line.IndexOf("]") - index;
                if (len < 1)
                {
                    continue;
                }

                var ver = line.Substring(index + 1, len - 1);

                index = line.IndexOf("(");
                len = line.IndexOf(")") - index;
                url = line.Substring(index + 1, len - 1);
                if (url.StartsWith("../"))
                {
                    url = url.Replace("../", "");
                }

                key = currentPlatform + "_" + ver;
                allsURL[key] = url;
            }

            if (platform == "IOS")
            {
                platform = "iOS";
            }

            key = platform + "_" + from;
            if (allsURL.TryGetValue(key, out url))
            {
                fromABS = ReportBuildUtil.GetAllAB(url);
            }
            if (fromABS == null)
            {
                Console.WriteLine("from:{0} 版本不存在", from);
                return;
            }
            key = platform + "_" + to;
            if (allsURL.TryGetValue(key, out url))
            {
                toABS = ReportBuildUtil.GetAllAB(url);
            }

            if (toABS == null)
            {
                Console.WriteLine("to:{0} 版本不存在", to);
                return;
            }

            updateCrcNames.Clear();

            var files = new List<string>();
            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir)==false)
                {
                    continue;
                }
                var list = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                if (list.Length > 0)
                {
                    files.AddRange(list);
                }
            }

            var dic = new Dictionary<string, CategoryVO<FileInfo>>();
            var total = 0L;
            foreach (var file in files)
            {
                var f = new FileInfo(file);
                total += f.Length;
                var fileDir = Path.GetDirectoryName(file);
                var dirName = Path.GetFileName(fileDir);

                var name = Path.GetFileNameWithoutExtension(file);
                if (dirName.StartsWith(ABKey))
                {
                    updateCrcNames.Add(name);
                }

                CategoryVO<FileInfo> categoryVo;
                if (dic.TryGetValue(dirName, out categoryVo) == false)
                {
                    dic[dirName] = categoryVo = new CategoryVO<FileInfo>();
                }

                categoryVo.add(f, f.Length);
            }

            formatABVO=new FormatABVO(fromABS,toABS,updateCrcNames);

            var lineStr = "";
            var sb = new StringBuilder();
            var lenStr = Util.GetSizeDesc(total);

            lineStr = string.Format("{0}\t资源更新总计 {1}", platform, lenStr);

            if (File.Exists(hpfFilePath))
            {
                var fileInfo=new FileInfo(hpfFilePath);
                lenStr = Util.GetSizeDesc(fileInfo.Length);
                lineStr += string.Format("\t压缩的大小({0})", lenStr);
            }

            sb.AppendLine(MD.Link("返回", "arts.md"));
            sb.AppendLine(MD.H1(lineStr));

            lineStr = formatCategorys(dic);
            sb.AppendLine(lineStr);

            //test;
            //sb.Clear();

           var gamePatch = new GamePatch(total);
            lineStr=gamePatch.Scan(formatABVO);
            if (string.IsNullOrEmpty(lineStr) == false)
            {
                sb.AppendLine("# 分类总结  ");
                sb.AppendLine(lineStr);
            }

           
            File.WriteAllText(mdFile, sb.ToString());
            Console.WriteLine("生成:{0}", mdFile);

            if (autoOpen)
            {
                Process.Start(mdFile);
            }
        }


        private string formatCategorys(Dictionary<string,CategoryVO<FileInfo>> dic)
        {
            var sb=new StringBuilder();
            var lenStr = "";
            var line="";

            foreach (var key in dic.Keys)
            {
                var cate = dic[key];
                var files = cate.items;
                files.Sort(ReportBuildUtil.FileSizeSort);
                lenStr = Util.GetSizeDesc(cate.totalSize);

                line = MD.H2("{0}\t{1}",key, lenStr);
                sb.AppendLine(line);
                sb.AppendLine("<details>");
                sb.AppendLine($"<summary>assets:{files.Count}</summary>\n");

                if (key.StartsWith(ABKey) == false)
                {
                    files.ForEach(ab =>
                    {
                        var name = Path.GetFileNameWithoutExtension(ab.Name);

                        var rawName = "";
                        if (CrcUtil.TryGetValue(name, out rawName))
                        {
                            name += "\t(" + rawName + ")";
                        }

                        lenStr = Util.GetSizeDesc(ab.Length);
                        line = MD.Li1("{0}\t\t{1}", name, lenStr);
                        sb.AppendLine(line);
                    });
                }
                else
                {
                    files.ForEach(ab =>
                    {
                        var name = Path.GetFileNameWithoutExtension(ab.Name);
                        var tsb = formatABVO.FormatMD(name, ab);
                        if (string.IsNullOrEmpty(tsb) == false)
                        {
                            sb.AppendLine(tsb);
                        }
                    });
                }

                sb.AppendLine("</details>\n\n---");
            }

            return sb.ToString();
        }


    }
}