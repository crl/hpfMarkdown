using hpfMarkdown.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace hpfMarkdown
{
    public class GamePatch
    {
        private long total = 0;
        public GamePatch(long total)
        {
            this.total = total;
        }
        public class Node : Dictionary<string, Node>
        {
            public string key;
            public long nodeSize = 0;
            public long fileSize = 0;
            public List<ReportAssetBundle> files = new List<ReportAssetBundle>();

            internal void AddFile(ReportAssetBundle value)
            {
                fileSize += value.GetRawLength();
                files.Add(value);
            }

            public void Sort()
            {
                files.Sort((a, b) =>
                {
                    var dis = b.GetRawLength() - a.GetRawLength();
                    if (dis == 0)
                    {
                        return 0;
                    }
                    return dis > 0 ? 1 : -1;
                });

                var list = this.Values.ToList();
                list.Sort((a, b) =>
                {
                    var dis = b.nodeSize - a.nodeSize;
                    if (dis == 0)
                    {
                        return 0;
                    }
                    return dis > 0 ? 1 : -1;
                });

                this.Clear();
                foreach (var node in list)
                {
                    this.Add(node.key, node);
                }
            }
        }

        private int min = 2;
        private int minFile = 3;
        private MDStringBuilder sb = new MDStringBuilder();

        private Dictionary<string, string> routerDic = new Dictionary<string, string>()
        {
            ["UIModule"]= "_Assets/RawRes/UI/",
            ["ShaderModule"] = "_Assets/RawRes/ShaderModule/",
            ["_Assets/RawRes/Prefab/UIEffect"] = "_Assets/RawRes/UI",
            ["_Assets/RawRes/Prefab/UI"] = "_Assets/RawRes/UI",
            ["_Assets/RawRes/UIIcon"] = "_Assets/RawRes/UI",
            ["_Assets/RawRes/UITexture"] = "_Assets/RawRes/UI",
            ["_Assets/RawRes/ClientLuaPlugin"] = "_Assets/RawRes/UI",
            

            ["_Assets/RawRes/SceneRes"] = "_Assets/RawRes/Scenes",
            ["_Assets/RawRes/Configs/GameScene"] = "_Assets/RawRes/Scenes",
            ["_Assets/RawRes/Configs/Chapter"] = "_Assets/RawRes/Scenes",

            ["_Assets/RawRes_O/UnityScenes"] = "_Assets/RawRes/Scenes",
            ["_Assets/RawRes_O/Scenes"] = "_Assets/RawRes/Scenes",
            ["_Assets/RawRes/Scenes"] = "_Assets/RawRes/Scenes",
        };

        private Dictionary<string, MermaidVO> mermaidDic = new Dictionary<string, MermaidVO>()
        {
            ["UI"] =new MermaidVO("ui"),
            ["Scenes"] = new MermaidVO("场景"),
            ["Prefab"] = new MermaidVO("角色/剧情"),
            ["Effect"] = new MermaidVO("特效")
        };


        internal string Scan(FormatABVO value)
        {
            var abs = value.GetUpdateABS();

            var rootNode = new Node();

            foreach (var key in abs.Keys)
            {
                var ab = abs[key];
                var originName = ab.GetOriginName();

                foreach (var prefixKey in routerDic.Keys)
                {
                    if (originName.StartsWith(prefixKey))
                    {
                        //Console.WriteLine("{0} match: {1}", originName, prefixKey);
                        originName = originName.Replace(prefixKey, routerDic[prefixKey]);
                        break;
                    }
                }

                var folderSplit = originName.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                var ext = Path.GetExtension(originName);
                var len = folderSplit.Length;
                if (string.IsNullOrEmpty(ext)==false)
                {
                    len = len - 1;
                }
                var parentNode = rootNode;
                for (int i = 0; i < len; i++)
                {
                    Node subNode = null;
                    var itemKey = folderSplit[i];
                    if (itemKey == "RawRes_O")
                    {
                        itemKey = "RawRes";
                    }

                    if (parentNode.TryGetValue(itemKey, out subNode) == false)
                    {
                        parentNode[itemKey] = subNode = new Node();
                        subNode.key = itemKey;
                    }

                    subNode.nodeSize += ab.GetRawLength();
                    parentNode = subNode;
                }

                parentNode.AddFile(ab);
            }

            merge(rootNode);


            sb.Clear();

            FormatNode(rootNode, String.Empty, string.Empty, 0);
            formatMermaid();

            return sb.ToString();
        }

        void formatMermaid()
        {
            sb.AppendLine("```mermaid");
            sb.AppendLine("pie title 总计 {0}", Util.GetSizeDesc(total, false));

            var other = 100;
            foreach (var key in mermaidDic.Keys)
            {
                var mvo = mermaidDic[key];
                if (mvo.node != null)
                {
                    var des = mvo.GetSizeDesc(false);
                    float v = mvo.node.nodeSize / (1.0f * total);
                    int percent=(int)(v*100);
                    other -= percent;

                    sb.AppendLine("\"{0}\t({2}\t{1}%)\":{1}", mvo.title, percent, des);
                }
            }

            sb.AppendLine("\"其它\t({0}%)\":{0}", other);
            sb.AppendLine("```");
        }


        /*public static List<string> SpecialDirs = new List<string>()
        {
            "Entities", "SceneRes", "Scenes", "UnityScenes", "Configs", "Storyline", "Effect",
            "Packages"
        };*/

        void FormatNode(Node node, string key, string prefix, uint intend = 0)
        {
            var line = "";
            var oldIntend = sb.Intend;
            sb.Intend = intend;

            var hasKey = string.IsNullOrEmpty(key) == false;
            if (hasKey)
            {

                if (mermaidDic.TryGetValue(key, out MermaidVO m))
                {
                    m.node = node;
                }

                prefix = "";
                line = string.Format("## {0}{1} \t {2}", prefix, key, Util.GetSizeDesc(node.nodeSize));
                sb.AppendLine(line);
                intend = sb.Intend++;

                prefix += key + "/";
            }

            var len = node.Keys.Count;

            if (len > 0)
            {
                if (len > min)
                {
                    sb.AppendLine("<details>");
                    line = string.Format("<summary>...文件夹:{0}</summary>", len);
                    sb.AppendLine(line);
                    //多来一空格
                    sb.AppendLine("");
                }

                foreach (var subKey in node.Keys)
                {
                    FormatNode(node[subKey], subKey, prefix, intend + 1);
                }

                if (len > min)
                {
                    sb.AppendLine("</details>");
                    //多来一空格
                    sb.AppendLine("");
                }
            }

            len = node.files.Count;
            if (len > 0)
            {

                if (len > minFile)
                {
                    sb.AppendLine("<details>");
                    line = string.Format("<summary>...文件:{0} {1}</summary>", len, Util.GetSizeDesc(node.fileSize));
                    sb.AppendLine(line);
                    //多来一空格
                    sb.AppendLine("");
                }

                foreach (var reportAssetBundle in node.files)
                {
                    line = $"[{reportAssetBundle.GetOriginName()}](#{reportAssetBundle.Name})";
                    line = MD.Color(line, Color.Green);
                    line += string.Format("\t {0}<br/>", Util.GetSizeDesc(reportAssetBundle.GetRawLength()));
                    sb.AppendLine(line);
                }

                if (len > minFile)
                {
                    sb.AppendLine("</details>");
                    //多来一空格
                    sb.AppendLine("");
                }
            }

            sb.Intend = oldIntend;
        }



        private void merge(Node parent)
        {
            var keys = parent.Keys.ToList();
            for (int i = 0,len=keys.Count; i < len; i++)
            {
                var key = keys[i];
                var node = parent[key];
                merge(node);

                var fileCount = node.files.Count;
                var count = node.Keys.Count;
                if (count == 1 && fileCount==0)
                {
                    var nodeKey = node.key;
                    var b=parent.Remove(nodeKey);
                    if (b == false)
                    {
                        Console.WriteLine("error:{0} {1}", nodeKey,key);
                    }

                    foreach (var subKey in node.Keys)
                    {
                        var subNode = node[subKey];
                        var newKey = nodeKey + "/" + subKey;
                        keys.Add(newKey);
                        len++;
                        subNode.key = newKey;
                        parent[newKey] = subNode;
                    }
                }
                node.Sort();
            }

           
        }

    }
}