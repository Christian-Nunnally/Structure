using System;

namespace Structure.IO.Persistence
{
    public class Node
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public string ParentID { get; set; }

        public int Rank { get; set; }

        public virtual Node Copy()
        {
            var copy = new Node();
            CopyTo(copy);
            return copy;
        }

        public void CopyTo(Node node)
        {
            node.ParentID = ParentID;
            node.Rank = Rank + 1;
        }
    }
}