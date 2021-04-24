namespace Structure.Code.Viewers
{
    public class TreeViewer : TreeEditor<Node>
    {
        public TreeViewer(string prompt, PersistedTree<Node> tree) : base(prompt, tree)
        {
        }
    }
}