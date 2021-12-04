﻿using System;

namespace Structure
{
    [Serializable]
    public class TaskItem : Node
    {
        private DateTime _completedDate;
        private string _name;

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
            item.Name = Name;
            item.CompletedDate = CompletedDate;
            item.ParentID = ParentID;
            item.Rank = Rank;
        }

        public virtual void DoTask(PersistedTree<TaskItem> tree)
        {
        }
    }
}