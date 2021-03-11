using Newtonsoft.Json;
using System;

namespace Structure
{
    public class PersistedValue<T>
    {
        protected string Name;
        private T _cachedValue;
        private bool _isCacheValid;

        public PersistedValue(string name)
        {
            Name = name;
        }

        public event Action<(T OldValue, T NewValue)> ValueChanged;

        public T Get()
        {
            if (_isCacheValid) return _cachedValue;
            _isCacheValid = true;
            try
            {
                return _cachedValue = JsonConvert.DeserializeObject<T>(FileIO.Get(Name));
            }
            catch
            {
                return _cachedValue = default;
            }
        }

        public void Set(T value)
        {
            if (Get().Equals(value)) return;
            var oldValue = Get();
            _cachedValue = value;
            FileIO.Set(Name, $"{value}");
            ValueChanged((oldValue, value));
        }
    }
}