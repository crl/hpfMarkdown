using System.Text;

namespace hpfMarkdown
{
    public static class Ext
    {
        public static void Clear(this StringBuilder sb)
        {
            sb.Remove(0, sb.Length);
            sb.Length = 0;
        }

        public static StringBuilder AppendLineMDIntend(this StringBuilder sb,string value,uint intend,params object[] args)
        {
            if (args.Length > 0)
            {
                value = string.Format(value, args);
            }

            if (intend < 1)
            {
                return sb.AppendLine(value);
            }


            if (intend > 0)
            {
                for (int i = 0; i < intend; i++)
                {
                    sb.Append(">");
                }
            }

            return sb.AppendLine(value);
        }
    }
}