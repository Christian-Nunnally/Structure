﻿using Structur.Program.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Structur.IO.Persistence
{
    public class NodeTree<TValue> : IEnumerable<KeyValuePair<string, TValue>> where TValue : Node
    {
        public NodeTree()
        {
            NodeRemoved += x => CountChanged?.Invoke();
        }

        protected Dictionary<string, TValue> Dictionary { get; } = new();
        protected TValue[] Values => Dictionary.Values.ToArray();

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

        public IList<TValue> GetChildren(string parent, bool recursive = false)
        {
            var children = Values.Where(x => x.ParentID == parent).ToList();
            if (recursive) children.AddRange(children.SelectMany(x => GetChildren(x.ID)));
            if (parent == null) children.AddRange(Values.Where(ParentHasBeenDeleted));
            return children.OrderBy(x => x.Rank).ToList();
        }

        private bool ParentHasBeenDeleted(TValue x) => x.ParentID != null && this[x.ParentID] == null;
    }
}