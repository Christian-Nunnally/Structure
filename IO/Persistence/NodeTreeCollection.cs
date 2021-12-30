using System.Collections;
using System.Collections.Generic;

namespace Structure
{
    public class NodeTreeCollection<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        protected Dictionary<string, TValue> Dictionary { get; } = new Dictionary<string, TValue>();

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

        public void Remove(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
            }
        }

        public void Set(TValue value) => Set(value?.ID, value);

        public void Remove(TValue value) => Remove(value?.ID);
    }
}