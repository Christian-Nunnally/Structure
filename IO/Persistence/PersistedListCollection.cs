using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedListCollection<T> : IEnumerable<T>
    {
        private readonly string _name;

        public PersistedListCollection(string name)
        {
            _name = name;
            List = JsonConvert.DeserializeObject<List<T>>(StructureFileIO.ReadFromFile(_name)) ?? new List<T>();
        }

        public int Count => List.Count;

        private List<T> List { get; }

        public void Add(T value)
        {
            List.Add(value);
            Save();
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

        public void Save() => StructureFileIO.Set(_name, JsonConvert.SerializeObject(List));
    }
}