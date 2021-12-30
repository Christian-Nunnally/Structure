using Newtonsoft.Json;
using System;

namespace Structure.Code
{
    [Serializable]
    public class ProgramInputData
    {
        public ProgramInputData(ConsoleKeyInfo keyInfo, DateTime time)
        {
            Key = (int)keyInfo.Key;
            Character = keyInfo.KeyChar;
            Modifiers = (int)keyInfo.Modifiers;
            Time = time;
        }

        public int Key { get; set; }

        // TODO: Rename to 'Character'. This messes up previous serialization.
        [JsonProperty(PropertyName="Char")]
        public char Character { get; set; }

        public int Modifiers { get; set; }

        public DateTime Time { get; set; }

        public ConsoleKeyInfo GetKeyInfo()
        {
            var modifiers = (ConsoleModifiers)Modifiers;
            return new ConsoleKeyInfo(
                Character,
                (ConsoleKey)Key,
                modifiers.HasFlag(ConsoleModifiers.Shift),
                modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control));
        }
    }
}