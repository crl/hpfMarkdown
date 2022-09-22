using System;
using System.Xml.Serialization;
using hpfMarkdown.utils;

namespace hpfMarkdown
{
    public interface INameable
    {
        string Name { get; }
    }

    [Serializable]
    [XmlRoot(ElementName = "AssetBundle")]
    public class ReportAssetBundle
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("OriginName")]
        public string OriginName;

        [XmlAttribute("MD5")]
        public string MD5;

        [XmlAttribute("Length")]
        public string Length;

        [XmlElement("Asset")]
        public ReportAsset[] assets;

        [XmlElement("DependencyAB", IsNullable = true)]
        public DependencyAB[] dependencyAB;



        private long _rawLength=-1;

        public long GetRawLength()
        {
            if (_rawLength == -1)
            {
                var lenStr = this.Length;
                _rawLength = 0L;
                if (lenStr.EndsWith("K"))
                {
                    lenStr = lenStr.Replace("K", "");
                    _rawLength = long.Parse(lenStr) * 1024;
                }
                else
                {
                    _rawLength = long.Parse(lenStr);
                }
            }

            return _rawLength;
        }
        private string _originName;
        public string GetOriginName()
        {
            if (_originName == null)
            {
                _originName = this.OriginName;
                var index = _originName.LastIndexOf("_Assets/");
                if (index > 0)
                {
                    _originName = _originName.Substring(index);
                }
                return _originName;
            }

            return _originName;
        }
    }

    [Serializable]
    public class ReportAsset: Nameable
    {

        [XmlAttribute("Length")]
        public string Length;

        [XmlAttribute("BeDepended")]
        public string BeDepended;

        [XmlAttribute("MD5")]
        public string MD5;

        [XmlAttribute("MetaMD5")]
        public string MetaMD5;

        [XmlAttribute("LastModifyTime")]
        public string LastModifyTime;

        [XmlAttribute("MetaLastModifyTime")]
        public string MetaLastModifyTime;
    }

    [Serializable]
    public class DependencyAB:Nameable
    {
    }

    [Serializable]
    public class Nameable
    {
        [XmlAttribute("Name")]
        public string Name;
    }
}