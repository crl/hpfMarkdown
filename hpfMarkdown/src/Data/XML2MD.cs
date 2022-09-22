using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace hpfMarkdown
{
    public class XML2MD
    {
        public string version
        {
            get;
            private set;
        }

        private Dictionary<string, ReportAssetBundle> _allAB = new Dictionary<string, ReportAssetBundle>();
        public bool TryGetValue(string name, out ReportAssetBundle rab)
        {
            return _allAB.TryGetValue(name, out rab);
        }

        public StringBuilder Format(string filePath, bool saveIt = false)
        {
            var xml = new XmlDocument();
            xml.Load(filePath);

            var versionNode = xml.SelectSingleNode("UnityGameFramework/BuildReport/Summary/ApplicableGameVersion");
            var versionSplit = versionNode.InnerText.Split('.');
            var len = versionSplit.Length;
            this.version = versionSplit[len - 2] + "." + versionSplit[len - 1];

            var sb = new StringBuilder();
            var list = xml.SelectSingleNode("UnityGameFramework/BuildReport/AssetBundles");
            foreach (XmlNode node in list)
            {
                XmlSerializer xs = new XmlSerializer(typeof(ReportAssetBundle));
                using (var reader = new StringReader(node.OuterXml))
                {
                    var rab = (ReportAssetBundle) xs.Deserialize(reader);
                    _allAB[rab.Name] = rab;
                    FormatAB(rab, null, sb);
                }
            }

            if (saveIt && sb.Length > 0)
            {
                var mdFile = filePath.Replace(".xml", ".md");
                File.WriteAllText(mdFile, sb.ToString());
            }
            return sb;
        }

        public static StringBuilder FormatAB(ReportAssetBundle rab, HashSet<string> updateCrcNames, StringBuilder sb = null)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }

            var orgi = rab.OriginName;
            var index = orgi.LastIndexOf("_Assets/");
            if (index > 0)
            {
                orgi = orgi.Substring(index);
            }

            var name = rab.Name;
            sb.AppendLine($"## {name}");

            var lenStr = rab.Length;
            var lenInt = 0L;
            if (lenStr.EndsWith("K"))
            {
                lenStr = lenStr.Replace("K", "");
                lenInt = long.Parse(lenStr) * 1024;
            }
            else
            {
                lenInt = long.Parse(lenStr);
            }
            lenStr = getSizeDesc(lenInt);

            sb.AppendLine($"### <a name='{orgi}'>{orgi}</a>  {lenStr}");
            sb.AppendLine("<details>");

            var depCount = 0;
            if (rab.dependencyAB != null)
            {
                depCount = rab.dependencyAB.Length;
            }

            var total = string.Format("assets:{0} deps:{1}", rab.assets.Length, depCount);
            sb.AppendLine($"<summary>{total}</summary>\n");
            foreach (var asset in rab.assets)
            {
                sb.AppendLine($">1. {asset.Name}  ");
            }

            if (depCount > 0)
            {
                foreach (var dependency in rab.dependencyAB)
                {
                    var depName = dependency.Name;
                    if (updateCrcNames == null || updateCrcNames.Contains(depName))
                    {
                        sb.AppendLine($">* [{depName}](#{depName})  ");
                    }
                    else
                    {
                        sb.AppendLine($">* {depName}  ");
                    }
                }
            }
            sb.AppendLine("</details>\n\n---");
            return sb;
        }

   
        public static string getSizeDesc(long total)
        {
            var kOrMG = "K";
            double size_k = Math.Round(total / 1024.0, 2);
            var size = size_k;
            if (size_k > 900f)
            {
                double size_mb = Math.Round(total / 1024.0 / 1024.0, 2);
                kOrMG = "M";
                size = size_mb;
                if (size_mb > 900f)
                {
                    size = Math.Round(size_mb / 1024.0, 2);
                    kOrMG = "G";
                }
            }

            return size + kOrMG;
        }
    }
}