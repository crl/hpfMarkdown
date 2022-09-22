using System.Collections.Generic;

namespace hpfMarkdown
{
    public class CategoryVO<T>
    {
        public long totalSize = 0;
        public string name;
        public string fullName;
        public List<T> items = new List<T>();

        public void add(T f, long size)
        {
            totalSize += size;
            items.Add(f);
        }
    }
}