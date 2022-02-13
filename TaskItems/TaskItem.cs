using Structure.IO;
using Structure.IO.Persistence;
using System;
using System.Diagnostics.Contracts;

namespace Structure.TaskItems
{
    [Serializable]
    public class TaskItem : Node
    {
        private DateTime _completedDate;
        private string _name;

        public string CopiedFromID { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CompletedDate
        {
            get => _completedDate;
            set
            {
                if (_completedDate != value)
                {
                    _completedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string ToString() => Name;

        public virtual TaskItem Copy()
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
            io.PromptOptions(
                $"Complete task {Name}?",
                true,
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