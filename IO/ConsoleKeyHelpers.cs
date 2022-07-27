using Structure.Structure.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            if (character == '/')
                return new ConsoleKeyInfo('/', ConsoleKey.Divide, false, false, false);
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

        public static Dictionary<ConsoleKeyInfo, UserAction> CreateUserActionToConsoleKeyMap(UserAction[] options)
        {
            var keys = new List<(ConsoleKeyInfo Key, UserAction Action)>();
            options.Where(x => x.HotkeyOverridden).All(x => keys.Add((x.Hotkey, x)));
            foreach (var option in options.Where(x => !x.HotkeyOverridden))
            {
                var possibleKeys = $"{option.Description.ToLowerInvariant()}abcdefghijklmnopqrstuvwxyz1234567890";
                var possibleKeyCharacters = possibleKeys.Select(ConvertCharToConsoleKey).Select(x => x.KeyChar).ToList();
                for (int i = 0; i < possibleKeys.Length; i++)
                {
                    if (char.IsWhiteSpace(possibleKeys[i])) continue;
                    if (!keys.Any(x => x.Key.KeyChar == possibleKeyCharacters[i]))
                    {
                        var consoleKeyInfo = ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]);
                        if (consoleKeyInfo.Key >= ConsoleKey.NumPad0 && consoleKeyInfo.Key <= ConsoleKey.NumPad9)
                        {
                            keys.Add((new ConsoleKeyInfo($"{(int)consoleKeyInfo.Key - ConsoleKey.NumPad0}"[0], ConsoleKey.D0 + (consoleKeyInfo.Key - ConsoleKey.NumPad0), false, false, false), option));
                        }
                        keys.Add((consoleKeyInfo, option));
                        option.Hotkey = consoleKeyInfo;
                        break;
                    }
                }
            }
            return keys.ToDictionary(x => x.Key, x => x.Action);
        }
    }
}
