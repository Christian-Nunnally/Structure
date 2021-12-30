using System.Collections.Generic;

namespace Structure
{
    internal class ListViewer
    {
        private readonly List<object> _orignalList;
        private readonly int _itemsAtATime;
        private readonly string _title;
        private List<object> _listCopy;

        public ListViewer(string title, List<object> list, int itemsAtATime)
        {
            _orignalList = list;
            _itemsAtATime = itemsAtATime;
            _title = title;
        }

        public void ViewList(StructureIO io)
        {
            if (_listCopy == null)
            {
                _listCopy = new List<object>(_orignalList);
            }
            io.Write(_title);
            for (int i = 0; i < _itemsAtATime && i < _listCopy.Count; i++)
            {
                io.Write($"{_listCopy[i]}");
            }
            io.ReadKey(k => ShowListInteraction(io, k));
        }

        private void ShowListInteraction(StructureIO io, string key)
        {
            io.Clear();
            if (key == "{Escape}")
            {
                _listCopy = null;
                return;
            }
            if (key == "{Enter}")
            {
                if (_listCopy.Count <= 1)
                {
                    _listCopy = null;
                    return;
                }
                _listCopy.RemoveAt(0);
                ViewList(io);
            }
        }
    }
}