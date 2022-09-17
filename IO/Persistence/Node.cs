using System;

namespace Structur.IO.Persistence
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

        public void SwapRanks(Node node) => (node.Rank, Rank) = (Rank, node.Rank);
    }
}