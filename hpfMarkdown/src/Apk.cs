using hpfMarkdown.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace hpfMarkdown
{
    public class Apk
    {
        Dictionary<string, ReportAssetBundle> toABS = null;

        private FormatABVO formatABVO;
        public void Run(string rootDir,string type)
        {
            var dir = rootDir + "/" + HpfsExport.EXPORTS;
            if (Directory.Exists(dir) && type=="-2")
            {
                rootDir = dir;
            }
            HpfsExport.Run(rootDir);

            var files = Directory.GetFiles(rootDir, "*_BuildReport.xml", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                MessageBox.Show("没有找到BuildReport文件,无法分析");
                return;
            }

            var url = files[0];
            toABS = ReportBuildUtil.GetAllAB(url);

            dir = rootDir;
            files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

            var updateCrcNames=new HashSet<string>();

            var dic = new Dictionary<string, CategoryVO<FileInfo>>();
            var total = 0L;
            foreach (var file in files)
            {
                var f = new FileInfo(file);
                if (f.Extension != "")
                {
                    continue;
                }
                total += f.Length;
                var fileDir = Path.GetDirectoryName(file);
                var dirName = Path.GetFileName(fileDir);

                var name = Path.GetFileNameWithoutExtension(file);

                updateCrcNames.Add(name);

                CategoryVO<FileInfo> categoryVo;
                if (dic.TryGetValue(dirName, out categoryVo) == false)
                {
                    dic[dirName] = categoryVo = new CategoryVO<FileInfo>();
                }

                categoryVo.add(f, f.Length);
            }

            formatABVO=new FormatABVO(null,toABS,updateCrcNames);

            var lineStr = "";
            var sb = new StringBuilder();
            var lenStr = Util.GetSizeDesc(total);

            lineStr = string.Format("资源总计 {0}", lenStr);
            sb.AppendLine(MD.H1(lineStr));

            lineStr = formatCategorys(dic);
            sb.AppendLine(lineStr);


            var mdFile = rootDir + "/art.md";
            File.WriteAllText(mdFile, sb.ToString());
            Console.WriteLine("生成:{0}", mdFile);

            Process.Start(mdFile);

        }

        private string formatCategorys(Dictionary<string, CategoryVO<FileInfo>> dic)
        {
            var sb = new StringBuilder();
            var lenStr = "";
            var line = "";

            foreach (var key in dic.Keys)
            {
                var cate = dic[key];
                var files = cate.items;
                files.Sort(ReportBuildUtil.FileSizeSort);
                lenStr = Util.GetSizeDesc(cate.totalSize);

                line = MD.H2("{0}\t{1}", key, lenStr);
                sb.AppendLine(line);
                sb.AppendLine("<details>");
                sb.AppendLine($"<summary>assets:{files.Count}</summary>\n");


                files.ForEach(ab =>
                {
                    var name = Path.GetFileNameWithoutExtension(ab.Name);
                    var tsb = formatABVO.FormatMD(name, ab);
                    if (tsb.Length > 0)
                    {
                        sb.AppendLine(tsb);
                    }
                });

                sb.AppendLine("</details>\n\n---");
            }
            return sb.ToString();
        }
    }
}