using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedListCollection<T> : IEnumerable<T>
    {
        private readonly string _name;
        private readonly bool autoSave;

        public PersistedListCollection(string name, bool autoSave = false)
        {
            _name = name;
            this.autoSave = autoSave;
            List = JsonConvert.DeserializeObject<List<T>>(FileIO.ReadFromFile(_name)) ?? new List<T>();
        }

        public int Count => List.Count;

        private List<T> List { get; }

        public void Add(T value)
        {
            List.Add(value);
            if (autoSave) Save();
        }

        public void Remove(T value)
        {
            List.Remove(value);
        }

        public void Clear()
        {
            List.Clear();
        }

        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        public void Save() => FileIO.Set(_name, JsonConvert.SerializeObject(List));
    }
}