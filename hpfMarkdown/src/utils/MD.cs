using System.Text;

namespace hpfMarkdown
{
    public class Color
    {
        public static string Blue = "#4183c4";
        public static string Green = "green";
        public static string Red = "red";
        public static string Pink = "pink";
    }
    public class MD
    {
        private static StringBuilder sb=new StringBuilder();

        public static string Color(string value, string color, params object[] args)
        {
            var str = string.Format(value, args);

            return $"<font color='{color}'>{str}</font>";
        }
        public static string H1(string value, params object[] args)
        {
            return H(value, 1, args);
        }
        public static string H2(string value, params object[] args)
        {
            return H(value, 2, args);
        }
        public static string H3(string value, params object[] args)
        {
            return H(value, 3, args);
        }
        public static string H4(string value, params object[] args)
        {
            return H(value, 4, args);
        }
        public static string H(string value, uint size = 1, params object[] args)
        {
            if (size < 1)
            {
                size = 1;
            }
            sb.Clear();
            for (int i = 0; i < size; i++)
            {
                sb.Append("#");
            }
            var str = string.Format(value, args);
            sb.Append($" {str}");
            return sb.ToString();
        }
        public static string Details(string value, string summary,uint size, params object[] args)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            sb.Clear();
            var prefix = string.Empty;
            if (size > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    sb.Append(">");
                }

                prefix = sb.ToString() + " ";
            }

            sb.Clear();
            sb.AppendLine(prefix+"<details>");
            if (string.IsNullOrEmpty(summary) == false)
            {
                sb.AppendLine($"{prefix}<summary>{summary}</summary>\n");
            }
            var str = string.Format(value, args);
            sb.AppendLine(str);
            sb.AppendLine(prefix + "</details>\n");

            return sb.ToString();
        }
        public static string Del(string value, params object[] args)
        {
            var str = string.Format(value, args);
            return $"~~{str}~~";
        }
        public static string Sup(string value, params object[] args)
        {
            var str = string.Format(value, args);
            return $"<sup>{str}</sup>";
        }
        public static string Sub(string value, params object[] args)
        {
            var str = string.Format(value, args);
            return $"<sub>{str}</sub>";
        }
        public static string Anchor(string value,string id=null, params object[] args)
        {
            var str = string.Format(value, args);
            if (string.IsNullOrEmpty(id))
            {
                id = str;
            }
            return $"<span id='{id}'>{str}</span>";
        }

        public static string Link(string value, string uri=null,bool isAnchor=false, params object[] args)
        {
            var str= string.Format(value, args);

            if (string.IsNullOrEmpty(uri))
            {
                uri = str;
            }

            if (isAnchor)
            {
                return $"[{str}](#{uri})";
            }
            return $"[{str}]({uri})";
        }

        public static string Li1(string value, params object[] args)
        {
            return Li(value, 1, args);
        }
        public static string Li(string value,uint size=0,params object[] args)
        {
            var str=string.Format(value, args);
            sb.Clear();
            for (int i = 0; i < size; i++)
            {
                sb.Append(">");
            }

            return $"{sb.ToString()} 1. {str}  ";
        }

        public static string Pg(string value, uint size = 1, params object[] args)
        {
            var str = string.Format(value, args);
            if (size < 1)
            {
                return str;
            }
            sb.Clear();
            for (int i = 0; i < size; i++)
            {
                sb.Append(">");
            }


            return $"{sb.ToString()} {str}  ";
        }

        public static string Lu(string value, uint size = 0, params object[] args)
        {
            var str = string.Format(value, args);
            sb.Clear();
            for (int i = 0; i < size; i++)
            {
                sb.Append(">");
            }

            return $"{sb.ToString()} - {str}  ";
        }
    }
}