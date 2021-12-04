using System;
using System.Linq;

namespace Structure
{
    public class Weight : Module
    {
        private static readonly PersistedList<FloatRecord> _weights = new PersistedList<FloatRecord>("Weights");

        private UserAction _action;

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.W, _action);
        }

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.W, new UserAction("Record weight", Prompt));
        }

        private void Prompt()
        {
            IO.WriteNoLine("Enter your current weight (lbs): ");
            IO.Read(RecordWeight);
        }

        private void RecordWeight(string input)
        {
            if (float.TryParse(input, out var weight))
            {
                if (!_weights.AsEnumerable().Any(x => x.Date.Date == DateTime.Today))
                {
                    _weights.Add(new FloatRecord
                    {
                        Value = weight,
                        Description = "Weight (lbs)"
                    });
                    IO.News($"Weight recorded: {weight} (lbs)");
                    Data.Points += 2;
                    Data.XP += 2;
                    if (_weights.Count % 5 == 0)
                    {
                        IO.News($"{_weights.Count} weights recorded! Bonus reward.");
                        Data.XP += 5;
                        Data.Points += 5;
                    }
                }
            }
        }
    }
}