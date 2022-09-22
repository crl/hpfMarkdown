using hpfMarkdown.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace hpfMarkdown
{
    public class FormatABVO
    {
        private Dictionary<string, ReportAssetBundle> fABS;
        private Dictionary<string, ReportAssetBundle> tABS;

        private Dictionary<string, ReportAssetBundle> updateABS=new Dictionary<string, ReportAssetBundle>();

        public FormatABVO(Dictionary<string, ReportAssetBundle> f, Dictionary<string, ReportAssetBundle> t, HashSet<string> updateCrcNames)
        {
            this.fABS = f;
            this.tABS = t;

            foreach (var crc in updateCrcNames)
            {
                if (tABS.TryGetValue(crc, out ReportAssetBundle rab))
                {
                    updateABS[crc] = rab;
                }
            }
        }

        public Dictionary<string, ReportAssetBundle> GetUpdateABS()
        {
            return updateABS;
        }
        public bool IsUpdate(string crc)
        {
            return updateABS.ContainsKey(crc);
        }

        public bool TryGetValueT(string key, out ReportAssetBundle ab)
        {
            if (tABS != null)
            {
                return tABS.TryGetValue(key, out ab);
            }

            ab = null;
            return false;
        }
        public bool TryGetValueF(string key, out ReportAssetBundle ab)
        {
            if (fABS != null)
            {
                return fABS.TryGetValue(key, out ab);
            }

            ab = null;
            return false;
        }

        private StringBuilder FormatMD(ReportAssetBundle fAB, ReportAssetBundle tAB)
        {
            var sb = new StringBuilder();

            var orgi = tAB.GetOriginName();
            var name = tAB.Name;
            if (fAB == null)
            {
                orgi = MD.Color("{0} +", Color.Green, orgi);
            }

            var lenInt = tAB.GetRawLength();
            var lenStr = Util.GetSizeDesc(lenInt);
            var line = MD.Color(name, Color.Blue);
            line += $"\t{lenStr}\t{orgi}";
            line = MD.Anchor(line, name);
            sb.AppendLine(MD.H3(line));

            sb.AppendLine("<details>");

            var depCount = 0;
            if (tAB.dependencyAB != null)
            {
                depCount = tAB.dependencyAB.Length;
            }

            var total = string.Format("assets:{0}/{1}", tAB.assets.Length, depCount);
            if (fAB != null)
            {
                var oldDepCount = 0;
                if (fAB.dependencyAB != null)
                {
                    oldDepCount = fAB.dependencyAB.Length;
                }

                total += string.Format("\t\t old:{0}/{1}", fAB.assets.Length, oldDepCount);
            }
            sb.AppendLine($"<summary>{total}</summary>\n");

            var mergeInfo = Util.Merge(tAB.assets, fAB?.assets);

            foreach (var mergeVo in mergeInfo.list)
            {
                var asset = mergeVo.data;
                line = asset.Name;
                line = ReportBuildUtil.CheckMergeStr(mergeVo, line);

                var length = Util.GetSizeBySpliStr(asset.Length);
                if (length>0)
                {
                    line += "\t" + Util.GetSizeDesc(length);

                    var oldAsset = mergeVo.oldData;
                    if (oldAsset != null)
                    {
                        var oldLength = Util.GetSizeBySpliStr(oldAsset.Length);
                        if (oldLength != length)
                        {
                            line += string.Format("<sup>~~{0}~~</sup>", Util.GetSizeDesc(length));
                        }
                    }
                }

                var modify = asset.LastModifyTime;
                if (string.IsNullOrEmpty(modify) == false)
                {
                    if (DateTime.TryParse(modify, out DateTime t))
                    {
                        var deltaSpan = DateTime.Now - t;
                        modify = Util.GetTimeSpan(deltaSpan)+"\t"+modify;
                    }

                    var time= string.Format("{0}", modify);

                    modify = asset.MetaLastModifyTime;
                    if (string.IsNullOrEmpty(modify) == false)
                    {
                        if (DateTime.TryParse(modify, out DateTime tt))
                        {
                            var deltaSpan = DateTime.Now - tt;
                            modify = Util.GetTimeSpan(deltaSpan) + "\t" + modify;
                            time += string.Format(",\tmeta:{0}", modify);
                        }
                    }
                    line += string.Format("\t({0})", time);
                }

                sb.AppendLine(MD.Li(line));
            }

            if (depCount > 0)
            {
                sb.AppendLine("");
                var mergeDepInfo = Util.Merge(tAB.dependencyAB, fAB?.dependencyAB);
                foreach (var mergeVo in mergeDepInfo.list)
                {
                    var dependency = mergeVo.data;
                    var depName = dependency.Name;
                    line = depName;
                    if (IsUpdate(depName))
                    {
                        line = $"[{depName}](#{depName})";
                    }

                    if (TryGetValueT(depName, out var tab))
                    {
                        line += string.Format("\t\t({0})", tab.GetOriginName());
                    }
                    else if (TryGetValueF(depName, out var fab))
                    {
                        line += string.Format("\t\t\t\t(<sup>old</sup>{0})", fab.GetOriginName());
                    }

                    line = ReportBuildUtil.CheckMergeStr(mergeVo, line);
                    //变成列表
                    sb.AppendLine(MD.Li(line));
                }
            }

            sb.AppendLine("</details>\n\n---");
            return sb;
        }

        internal string FormatMD(string name,FileInfo ab)
        {
            if (tABS.TryGetValue(name, out var nab))
            {
                nab.Length = ab.Length.ToString();

                fABS.TryGetValue(name, out var oab);
                return FormatMD(oab, nab).ToString();
            }

            return null;
        }
    }
}