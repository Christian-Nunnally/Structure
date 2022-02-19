using System;
using System.Collections.Generic;
using System.Globalization;

namespace Structure.IO
{
    public static class ConsoleKeyHelpers
    {
        private static readonly Dictionary<char, ConsoleKeyInfo> _map = new Dictionary<char, ConsoleKeyInfo>();

        public static ConsoleKeyInfo ConvertCharToConsoleKey(char character)
        {
            if (_map.TryGetValue(character, out var key)) return key;
            _map.Add(character, ConvertCharToConsoleKeyHelper(character));
            return _map[character];
        }

        private static ConsoleKeyInfo ConvertCharToConsoleKeyHelper(char character)
        {
            if (int.TryParse($"{character}", out int result))
                return new ConsoleKeyInfo(character, ConsoleKey.NumPad0 + result, false, false, false);
            if (Enum.TryParse(character.ToString(CultureInfo.InvariantCulture), true, out ConsoleKey consoleKey))
                return new ConsoleKeyInfo(character, consoleKey, false, false, false);
            if (character == ' ')
                return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
            if (character == '.')
                return new ConsoleKeyInfo('.', ConsoleKey.OemPeriod, false, false, false);
            throw new InvalidOperationException($"Unable to convert '{character}' to ConsoleKey");
        }

        public static bool IsAlphanumeric(ConsoleKeyInfo key)
        {
            return char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == ' ' || key.Key == ConsoleKey.OemPeriod || key.Key == ConsoleKey.Decimal;
        }

        public static bool IsModifierPressed(ConsoleKeyInfo key)
        {
            return key.Modifiers.HasFlag(ConsoleModifiers.Control)
                || key.Modifiers.HasFlag(ConsoleModifiers.Alt);
        }
    }
}
