namespace Structure
{
    public class PersistedInt : PersistedValue<int>
    {
        public PersistedInt(string name) : base(name)
        {
            ValueChanged += IntChanged;
        }

        private void IntChanged((int OldValue, int NewValue) obj)
        {
            var difference = obj.NewValue - obj.OldValue;
            var prefix = difference > 0 ? "+" : "";
            IO.News($"{prefix}{difference} {Name}");
        }
    }
}