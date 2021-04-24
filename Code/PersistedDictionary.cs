using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        protected string _name;
        private Dictionary<string, TValue> _dictionary;

        public PersistedDictionary(string name)
        {
            _name = name;
        }

        protected Dictionary<string, TValue> Dictionary => _dictionary ?? (_dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(FileIO.Get(_name)) ?? new Dictionary<string, TValue>());

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
                value.PropertyChanged += ItemPropertyChanged;
                Dictionary[key] = value;
                Save();
            }
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        internal void Remove(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
                Save();
            }
        }

        private void Save() => FileIO.Set(_name, JsonConvert.SerializeObject(Dictionary));

        private void ItemPropertyChanged() => Save();
    }
}