namespace hpfMarkdown
{
    public class MermaidVO
    {
        public string title;
        public GamePatch.Node node;
        public MermaidVO(string title)
        {
            this.title = title;
        }

        public string GetSizeDesc(bool color=true)
        {
            if(node!=null)return Util.GetSizeDesc(node.nodeSize,color);

            return "";
        } 
    }
}