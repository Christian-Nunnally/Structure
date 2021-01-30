using System;

namespace Structure
{
    [Serializable]
    public class TaskItem : Node
    {
        public string Task;
        public int Rank;
        public DateTime CompletedDate;
    }
}