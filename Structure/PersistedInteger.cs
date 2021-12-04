namespace Structure
{
    public class PersistedInt : SerializeableValue<int>
    {
        public PersistedInt(string name) : base(name)
        {
        }

        public override void Set(int value)
        {
            var difference = value - _value;
            var prefix = difference > 0 ? "+" : "";
            IO.News($"{prefix}{difference} {Name} ({_value})");
            base.Set(value);
        }
    }
}