using System;
using System.Collections;
using System.Collections.Generic;

namespace Structure.IO.Persistence
{
    public class NodeTreeCollection<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        private int _oldCount;

        private void DidCountChange()
        {
            if (_oldCount != Dictionary.Count)
            {
                _oldCount = Dictionary.Count;
                CountChanged?.Invoke();
            }
        }
        protected Dictionary<string, TValue> Dictionary { get; } = new Dictionary<string, TValue>();
        
        public event Action CountChanged;

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
                DidCountChange();
            }
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        public void Remove(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
                DidCountChange();
            }
        }

        public void Set(TValue value) => Set(value?.ID, value);

        public void Remove(TValue value) => Remove(value?.ID);
    }
}