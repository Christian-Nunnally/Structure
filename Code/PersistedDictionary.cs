using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class PersistedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TValue : class
    {
        protected string _name;
        private Dictionary<TKey, TValue> _dictionary;

        public PersistedDictionary(string name)
        {
            _name = name;
        }

        public event Action<TKey, TValue> ItemRemoved;

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
                Dictionary[key] = value;
                FileIO.Set(_name, JsonConvert.SerializeObject(Dictionary));
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        internal void Remove(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var removed))
            {
                Dictionary.Remove(key);
                FileIO.Set(_name, JsonConvert.SerializeObject(Dictionary));
                ItemRemoved?.Invoke(key, removed);
            }
        }
    }
}