﻿namespace Structure
{
    public class PersistedTree<TValue> : PersistedDictionary<string, TValue> where TValue : Node
    {
        public PersistedTree(string name) : base(name)
        {
        }

        public void Set(TValue value) => Set(value.ID, value);

        internal void Remove(TValue value) => Remove(value.ID);
    }
}