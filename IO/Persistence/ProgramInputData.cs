using Newtonsoft.Json;
using System;

namespace Structur.IO.Input
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

        [JsonProperty(PropertyName = "K")]
        public int Key { get; set; }

        [JsonProperty(PropertyName="C")]
        public char Character { get; set; }

        [JsonProperty(PropertyName = "M")]
        public int Modifiers { get; set; }

        [JsonProperty(PropertyName = "T")]
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