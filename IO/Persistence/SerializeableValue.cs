using Newtonsoft.Json;

namespace Structure
{
    public class SerializeableValue<T>
    {
        protected string Name;
        protected T _value;

        public SerializeableValue(string name)
        {
            Name = name;
            try
            {
                _value = (T)JsonConvert.DeserializeObject<T>(FileIO.ReadFromFile(Name));
            }
            catch
            {
                _value = default;
            }
        }

        public T Get()
        {
            return _value;
        }

        public virtual void Set(T value)
        {
            _value = value;
        }

        public void Save()
        {
            FileIO.Set(Name, $"{_value}");
        }
    }
}