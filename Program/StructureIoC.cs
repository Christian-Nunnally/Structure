using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Program
{
    public class StructureIoC
    {
        private readonly Dictionary<Type, List<Func<object>>> _factories = new Dictionary<Type, List<Func<object>>>();

        public T Get<T>()
        {
            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException($"{typeof(T).FullName} not a registered type in the IoC container.");
            return (T)_factories[typeof(T)].First().Invoke();
        }

        public T[] GetAll<T>()
        {
            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException($"{typeof(T).FullName} not a registered type in the IoC container.");
            return _factories[typeof(T)].Select(x => (T)x.Invoke()).ToArray();
        }

        public void Register<T>(Func<object> factory)
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new List<Func<object>>());
            _factories[typeof(T)].Add(factory);
        }

        public void Register<T>()
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new List<Func<object>>());
            var singleInstance = Activator.CreateInstance<T>();
            _factories[typeof(T)].Add(() => singleInstance);
        }

        public void Register<T>(Type toType)
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new List<Func<object>>());
            var singleInstance = Activator.CreateInstance(toType);
            _factories[typeof(T)].Add(() => singleInstance);
        }
    }
}
