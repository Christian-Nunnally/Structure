using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure.IO.Persistence
{
    public class PersistedListCollection<T> : IEnumerable<T>
    {
        private readonly string _name;
        private List<T> _list;

        public PersistedListCollection(string name)
        {
            _name = name;
            HasBeenSaved = StructureFileIO.DoesFileExist(_name);
        }

        public bool HasBeenSaved { get; private set; }

        public int Count => List.Count;

        private List<T> List => _list ?? (_list = LoadList() ?? new List<T>());

        private List<T> LoadList()
        {
            return JsonConvert.DeserializeObject<List<T>>(StructureFileIO.ReadFromFile(_name));
        }

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