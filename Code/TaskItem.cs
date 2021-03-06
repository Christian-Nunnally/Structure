using System;

namespace Structure
{
    [Serializable]
    public class TaskItem : Node
    {
        private DateTime _completedDate;
        private string _task;

        public string Task
        {
            get => _task;
            set
            {
                if (_task != value)
                {
                    _task = value;
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

        public override string ToString() => Task;
    }
}