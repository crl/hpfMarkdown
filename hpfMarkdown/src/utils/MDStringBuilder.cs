using System.Text;

namespace hpfMarkdown.utils
{
    public class MDStringBuilder
    {
        private StringBuilder sb = new StringBuilder();
        public uint Intend = 0;


        public void AppendLine(string value, params object[] args)
        {
            if (args.Length > 0)
            {
                value = string.Format(value, args);
            }

            for (int i = 0; i < Intend; i++)
            {
                sb.Append(">");
            }

            sb.AppendLine(value);
        }

        public void Append(string value, bool isStart=false,params object[] args)
        {
            if (isStart)
            {
                for (int i = 0; i < Intend; i++)
                {
                    sb.Append(">");
                }
            }
            if (args.Length > 0)
            {
                value = string.Format(value, args);
            }
            sb.Append(value);
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public int Length
        {
            get { return sb.Length; }
        }

        public void Clear()
        {
            this.sb.Clear();
        }
    }
}