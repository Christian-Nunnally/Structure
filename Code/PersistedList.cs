using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedList<T> : IEnumerable<T>
    {
        private string _name;
        private List<T> _list;

        public PersistedList(string name)
        {
            _name = name;
        }

        public int Count => List.Count;

        private List<T> List => _list ?? (_list = JsonConvert.DeserializeObject<List<T>>(FileIO.Get(_name)) ?? new List<T>());

        public void Add(T value)
        {
            List.Add(value);
            Set();
        }

        public void Remove(T value)
        {
            List.Remove(value);
            Set();
        }

        public void Clear()
        {
            List.Clear();
            Set();
        }

        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        private void Set() => FileIO.Set(_name, JsonConvert.SerializeObject(List));
    }
}