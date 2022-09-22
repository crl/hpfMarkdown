using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace hpfMarkdown
{
    
    public class ReportVO
    {
        public string version;
        public string svnTag;
        public string target;
        public string reportPath;
        public Dictionary<string, string> inject;
        

        public string reportPathMD
        {
            get { return reportPath.Replace(".xml", ".md"); }
        }

        public string day
        {
            get
            {

                var _day = "";
                if (inject.TryGetValue("nowday", out _day) == false)
                {
                    _day = Path.GetDirectoryName(reportPath);
                    _day = Path.GetDirectoryName(_day);
                    _day = Path.GetFileName(_day);
                }

                return _day;
            }
        }
    }

    public class ReportNode:Dictionary<string,ReportNode>
    {
        public List<ReportVO> list = new List<ReportVO>();
    }

    public class BuildReportMd
    {
        public const string IndexMdFileSubfix = "index.md";
        public static void Run(string dir)
        {
            var files = Directory.GetFiles(dir, "inject.txt", SearchOption.AllDirectories);

            var list = new List<ReportVO>();
            foreach (var file in files)
            {
                var dic = new Dictionary<string, string>();
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var paris = line.Split('=');

                    dic[paris[0]] = paris[1];
                }

                var fullVersion = "";
                if (dic.TryGetValue("fullVersion", out fullVersion))
                {
                    var reportVO = new ReportVO();
                    reportVO.version = fullVersion;

                    var fileName = "";
                    if (dic.TryGetValue("FileName", out fileName))
                    {
                        var parentDir = Path.GetDirectoryName(file);
                        parentDir = parentDir.Replace("\\", "/").Replace(dir + "/", "");

                        reportVO.reportPath = string.Format("{0}/{1}_BuildReport.xml", parentDir, fileName);
                        if (File.Exists(reportVO.reportPath) == false)
                        {
                            continue;
                        }

                        /*var mdFile = file.Replace(".xml", ".md");
                        if (File.Exists(mdFile) == false)
                        {
                            var md = new XML2MD();
                            md.Format(reportVO.reportPath, true);
                        }*/
                    }

                    var fileNameSplis = fileName.Split('_');
                    reportVO.svnTag = fileNameSplis[1];
                    reportVO.target = fileNameSplis[2];
                    reportVO.inject = dic;

                    list.Add(reportVO);
                }
            }
            format(dir,list);

            Console.WriteLine("Complete index.md");
        }


        private static void format(string dir,List<ReportVO> list)
        {
            var sb = new StringBuilder();

            var root=new ReportNode();
            foreach (var reportVo in list)
            {
                ReportNode dayNode;
                var day = reportVo.day;
                if (root.TryGetValue(day, out dayNode) ==false)
                {
                    root[day] = dayNode = new ReportNode();
                }

                ReportNode tagNode;
                var tag = reportVo.svnTag;
                if (dayNode.TryGetValue(tag, out tagNode) == false)
                {
                    dayNode[tag] = tagNode = new ReportNode();
                }

                ReportNode targetNode;
                var target = reportVo.target;
                if (tagNode.TryGetValue(target, out targetNode) == false)
                {
                    tagNode[target] = targetNode = new ReportNode();
                }

                targetNode.list.Add(reportVo);
            }


            //day,tag,target,list
            sb.AppendLine(MD.Link("->更新表  ", "arts.md"));
            foreach (var day in root.Keys)
            {
                var dayNode = root[day];
                sb.AppendLine($"# {day}");
                foreach (var tag in dayNode.Keys)
                {
                    var tagNode = dayNode[tag];
                    //sb.AppendLine($"## {tag}");
                    foreach (var target in tagNode.Keys)
                    {
                        var tagetNode = tagNode[target];
                        sb.AppendLine($"####  {tag}_{target}");
                        foreach (var reportVo in tagetNode.list)
                        {
                            var temp = "[{0}](../{1})";
                            var line = "1. ";
                            line += string.Format(temp, reportVo.version, reportVo.reportPath);
                            sb.AppendLine(line);
                            var tDir=Path.GetDirectoryName(reportVo.reportPath);

                            line = "   + ";
                            line += string.Format(temp, "hpfAndModule", tDir + "/hpfAndModule.txt");
                            sb.AppendLine(line);
                            line = "   + ";
                            line += string.Format(temp, "HpfInfos", tDir + "/HpfInfos.json");
                            sb.AppendLine(line);
                            line = "   + ";
                            line += string.Format(temp, "...", tDir);
                            sb.AppendLine(line+"\n");

                        }
                    }
                }
            }

            var path = GetIndexMdPath();
            File.WriteAllText(path, sb.ToString());

        }

        public static string GetIndexMdPath()
        {
            var path = string.Format("{0}/{1}/{2}", Program.RootDir, HpfsExport.EXPORTS, IndexMdFileSubfix);
            return path;
        }
    }
}