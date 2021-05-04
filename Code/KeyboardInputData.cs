using System;

namespace Structure.Code
{
    [Serializable]
    public class KeyboardInputData
    {
        public KeyboardInputData(ConsoleKeyInfo keyInfo, DateTime time)
        {
            Key = (int)keyInfo.Key;
            Char = keyInfo.KeyChar;
            Modifiers = (int)keyInfo.Modifiers;
            Time = time;
        }

        public int Key { get; set; }

        public char Char { get; set; }

        public int Modifiers { get; set; }

        public DateTime Time { get; set; }

        public ConsoleKeyInfo GetKeyInfo()
        {
            var modifiers = (ConsoleModifiers)Modifiers;
            return new ConsoleKeyInfo(
                Char,
                (ConsoleKey)Key,
                modifiers.HasFlag(ConsoleModifiers.Shift),
                modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control));
        }
    }
}