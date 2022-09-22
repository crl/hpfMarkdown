using System.Collections.Generic;

namespace hpfMarkdown
{
    public enum MergeType
    {
        Def,
        Add,
        Remove,
        Change
    }

    public class MergeInfoVO<T>
    {
        public List<MergeVO<T>> list = new List<MergeVO<T>>();
        public int addCount = 0;
        public int removeCount = 0;
    }
    public class MergeVO<T>
    {
        public T data;
        public T oldData;
        public MergeType type;
    }
}