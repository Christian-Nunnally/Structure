using System;

namespace Structure
{
    public class Node
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        private int _rank;
        private string _parentID;

        public event Action PropertyChanged;

        public string ParentID
        {
            get => _parentID;
            set
            {
                if (_parentID != value)
                {
                    _parentID = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Rank
        {
            get => _rank;
            set
            {
                if (_rank != value)
                {
                    _rank = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged() => PropertyChanged?.Invoke();
    }
}