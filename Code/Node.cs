using System;

namespace Structure
{
    public class Node
    {
        public string ID = Guid.NewGuid().ToString();
        public string ParentID;
    }
}