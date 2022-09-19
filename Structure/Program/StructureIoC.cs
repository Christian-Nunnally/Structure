using System;
using System.Collections.Generic;
using System.Linq;

namespace Structur.Program
{
    public class StructureIoC
    {
        private readonly Dictionary<Type, Dictionary<string, Func<object>>> _factories = new();

        public T Get<T>()
        {
            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException($"{typeof(T).FullName} not a registered type in the IoC container.");
            return (T)_factories[typeof(T)].Last().Value.Invoke();
        }

        public T Get<T>(string key)
        {
            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException($"{typeof(T).FullName} not a registered type in the IoC container.");
            if (!_factories[typeof(T)].ContainsKey(key)) throw new InvalidOperationException($"{typeof(T).FullName} has no object registered with they key '{key}' in the IoC container.");
            return (T)_factories[typeof(T)][key].Invoke();
        }

        public T[] GetAll<T>()
        {
            if (!_factories.ContainsKey(typeof(T))) throw new InvalidOperationException($"{typeof(T).FullName} not a registered type in the IoC container.");
            return _factories[typeof(T)].Select(x => (T)x.Value.Invoke()).ToArray();
        }

        public void Register<T>(Func<object> factory) => Register<T>(factory, factory().GetType().Name);

        public void Register<T>(Func<object> factory, string tag)
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new Dictionary<string, Func<object>>());
            if (_factories[typeof(T)].ContainsKey(tag)) _factories[typeof(T)][tag] = factory;
            else _factories[typeof(T)].Add(tag, factory);
        }

        public void Register<T>() => Register<T>("singleInstance");

        public void Register<T>(string key)
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new Dictionary<string, Func<object>>());
            var instance = Activator.CreateInstance<T>();
            if (_factories[typeof(T)].ContainsKey(key)) _factories[typeof(T)][key] = () => instance;
            else _factories[typeof(T)].Add(key, () => instance);
        }

        public void Register<T>(Type toType)
        {
            if (!_factories.ContainsKey(typeof(T))) _factories.Add(typeof(T), new Dictionary<string, Func<object>>());
            var instance = Activator.CreateInstance(toType);
            if (_factories[typeof(T)].ContainsKey("singleInstance")) _factories[typeof(T)]["singleInstance"] = () => instance;
            else _factories[typeof(T)].Add("singleInstance", () => instance);
        }
    }
}
