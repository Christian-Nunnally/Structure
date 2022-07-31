using System;
using System.Collections;
using System.Collections.Generic;

namespace Structur.IO.Persistence
{
    public class NodeTree<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        public NodeTree()
        {
            NodeRemoved += x => CountChanged?.Invoke();
        }

        protected Dictionary<string, TValue> Dictionary { get; } = new Dictionary<string, TValue>();
        
        public event Action CountChanged;
        public event Action<TValue> NodeRemoved;

        public TValue this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public TValue Get(string key) => (key is object && Dictionary.ContainsKey(key)) ? Dictionary[key] : null;

        public void Set(string key, TValue value)
        {
            var oldValue = Get(key);
            if (oldValue != value)
            {
                Dictionary[key] = value;
                if (oldValue == null) CountChanged?.Invoke();
            }
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        public void Remove(string key)
        {
            if (key == null) return;
            if (Dictionary.ContainsKey(key))
            {
                var removedItem = Dictionary[key];
                Dictionary.Remove(key);
                NodeRemoved?.Invoke(removedItem);
            }
        }

        public void Set(TValue value) => Set(value?.ID, value);

        public void Remove(TValue value) => Remove(value?.ID);
    }
}