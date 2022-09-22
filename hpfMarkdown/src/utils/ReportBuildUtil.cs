using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace hpfMarkdown.utils
{
    public class ReportBuildUtil
    {
        private static Dictionary<string, Dictionary<string, ReportAssetBundle>> AllABDic;
        public static Dictionary<string, ReportAssetBundle> GetAllAB(string url)
        {
            if (AllABDic == null)
            {
                AllABDic = new Dictionary<string, Dictionary<string, ReportAssetBundle>>();
            }

            Dictionary<string, ReportAssetBundle> _allAB;

            if (AllABDic.TryGetValue(url, out _allAB) == false)
            {
                var n = DateTime.Now;
                var xml = new XmlDocument();
                xml.Load(url);
                _allAB = new Dictionary<string, ReportAssetBundle>();
                var list = xml.SelectSingleNode("UnityGameFramework/BuildReport/AssetBundles");
                foreach (XmlNode node in list)
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ReportAssetBundle));
                    using (var reader = new StringReader(node.OuterXml))
                    {
                        var rab = (ReportAssetBundle)xs.Deserialize(reader);
                        _allAB[rab.Name] = rab;
                    }
                }

                AllABDic[url] = _allAB;
                var time = DateTime.Now.Subtract(n);
                Console.WriteLine("parser: {0} cost: {1}", url, time.TotalMilliseconds);
            }

            return _allAB;
        }


        public static int FileSizeSort(FileInfo x, FileInfo y)
        {
            var dis = y.Length - x.Length;
            if (dis == 0)
            {
                return 0;
            }
            return dis > 0 ? 1 : -1;
        }

        public static string CheckMergeStr<T>(MergeVO<T> mergeVo, string line)
        {
            switch (mergeVo.type)
            {
                case MergeType.Add:
                    line = MD.Color($"{line} +", Color.Green);
                    break;
                case MergeType.Remove:
                    line = MD.Color($"~~{line}~~ -", Color.Red);
                    break;
            }
            return line;
        }
    }
}