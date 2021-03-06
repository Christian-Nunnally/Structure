using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TValue : Node
    {
        protected string _name;
        private Dictionary<TKey, TValue> _dictionary;

        public PersistedDictionary(string name)
        {
            _name = name;
        }

        protected Dictionary<TKey, TValue> Dictionary => _dictionary ?? (_dictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(FileIO.Get(_name)) ?? new Dictionary<TKey, TValue>());

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public TValue Get(TKey key) => (key is object && Dictionary.ContainsKey(key)) ? Dictionary[key] : null;

        public void Set(TKey key, TValue value)
        {
            if (Get(key) != value)
            {
                value.PropertyChanged += ItemPropertyChanged;
                Dictionary[key] = value;
                Save();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        internal void Remove(TKey key)
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