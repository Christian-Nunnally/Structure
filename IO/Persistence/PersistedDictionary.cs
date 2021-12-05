using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        protected string _name;

        public PersistedDictionary(string name)
        {
            _name = name;
            Dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(FileIO.ReadFromFile(_name)) ?? new Dictionary<string, TValue>();
        }

        protected Dictionary<string, TValue> Dictionary { get; }

        public TValue this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public TValue Get(string key) => (key is object && Dictionary.ContainsKey(key)) ? Dictionary[key] : null;

        public void Set(string key, TValue value)
        {
            if (Get(key) != value)
            {
                Dictionary[key] = value;
            }
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        public void Save()
        {
            FileIO.Set(_name, JsonConvert.SerializeObject(Dictionary));
        }

        public void Remove(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
            }
        }
    }
}