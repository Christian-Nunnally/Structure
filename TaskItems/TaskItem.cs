using Structure.IO;
using Structure.IO.Persistence;
using System;
using System.Diagnostics.Contracts;

namespace Structure.TaskItems
{
    [Serializable]
    public class TaskItem : Node
    {
        public string CopiedFromID { get; set; }

        public string Name { get; set; }

        public DateTime CompletedDate { get; set; }

        public override string ToString() => Name;

        public override Node Copy()
        {
            var copy = new TaskItem();
            CopyTo(copy);
            return copy;
        }

        public void CopyTo(TaskItem item)
        {
            Contract.Requires(item != null);
            item.Name = Name;
            item.CompletedDate = CompletedDate;
            item.ParentID = ParentID;
            item.Rank = Rank;
            item.CopiedFromID = ID;
        }

        public virtual bool CanDoTask(StructureIO io)
        {
            Contract.Requires(io != null);
            var can = false;
            io.ReadOptions(
                $"Complete '{Name}'?",
                false,
                new UserAction("No", () => can = false, ConsoleKey.N),
                new UserAction("Yes", () => can = true, ConsoleKey.Enter));
            return can;
        }

        public virtual void DoTask(DateTime completedTime, NodeTreeCollection<TaskItem> tree)
        {
            CompletedDate = completedTime;
            tree?.Remove(ID);
        }
    }
}