using System;

namespace Structure.IO.Persistence
{
    public class Node
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();

        public string ParentID { get; set; }

        public int Rank { get; set; }
    }
}