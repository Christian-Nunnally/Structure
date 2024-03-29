﻿using Structur.IO;
using Structur.IO.Persistence;
using System;
using System.Diagnostics.Contracts;

namespace Structur.TaskItems
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
                null,
                new UserAction("No", () => can = false, ConsoleKey.N),
                new UserAction("Yes", () => can = true, ConsoleKey.Enter));
            return can;
        }

        public virtual void DoTask(DateTime completedTime, NodeTree<TaskItem> tree)
        {
            CompletedDate = completedTime;
            tree?.Remove(ID);
        }
    }
}