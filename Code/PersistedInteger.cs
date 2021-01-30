namespace Structure
{
    public class PersistedInt
    {
        private string _name;
        private int? cachedValue;

        public PersistedInt(string name)
        {
            _name = name;
        }

        public int Get() => cachedValue ?? (cachedValue = int.TryParse(FileIO.Get(_name), out var x) ? x : 0) ?? 0;

        public void Set(int value)
        {
            if (Get() != value)
            {
                var difference = value - Get();
                var prefix = difference > 0 ? "+" : "";
                IO.News($"{prefix}{difference} {_name}");
                cachedValue = value;
                FileIO.Set(_name, $"{value}");
            }
        }
    }
}