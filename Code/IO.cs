using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structure
{
    public static class IO
    {
        private static readonly Stack<string> _textBuffers = new Stack<string>();
        private static readonly Dictionary<ConsoleKey, Action> _controlHotkeys = new Dictionary<ConsoleKey, Action>();
        private static readonly StringBuilder _textBuffer = new StringBuilder();

        public static void InitializeHotkey(ConsoleKey key, Action action)
        {
            _controlHotkeys.Add(key, action);
        }

        public static void Write(string text = "") => WriteNoLine($"{text}\n");

        public static void Write(char character) => Write($"{character}");

        public static void WriteNoLine(string text = "")
        {
            _textBuffer.Append(text);
            Console.Write(text);
        }

        public static void WriteNoLine(char character) => WriteNoLine($"{character}");

        public static void Clear()
        {
            _textBuffer.Clear();
            Console.Clear();
        }

        public static void ReadAny() => Read((line, key) => true, x => { }, false);

        public static void ReadLine(Action<string> continuation) => Read((line, key) => key.Key == ConsoleKey.Enter, continuation);

        public static void ReadKey(Action<string> continuation) => Read((line, key) => !IsModifierPressed(key), continuation);

        public static void Read(Func<string, ConsoleKeyInfo, bool> shouldExitPredicate, Action<string> continuation, bool echoRead = true)
        {
            ConsoleKeyInfo key;
            var lineBuilder = new StringBuilder();
            do
            {
                key = Console.ReadKey(true);
                ProcessReadKeyIntoLine(key, lineBuilder, echoRead);
            } while (!shouldExitPredicate(lineBuilder.ToString(), key));
            if (echoRead) Write();
            continuation(lineBuilder.ToString());
        }

        public static void PromptOptions(string prompt, params (char key, string description, Action action)[] options)
        {
            Write($"{prompt}\n");
            foreach (var (key, description, action) in options)
            {
                Write($"{key}: {description}");
            }
            ReadKey(PickOption);
            void PickOption(string result)
            {
                var (key, description, action) = options.FirstOrDefault(x => $"{x.key}" == result);
                action ??= options.Last().action;
                action();
            }
        }

        public static void ExecuteSubroutine(Action action)
        {
            _textBuffers.Push(_textBuffer.ToString());
            Clear();
            action();
            Clear();
            WriteNoLine(_textBuffers.Pop());
        }

        private static void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder lineBuilder, bool echoRead)
        {
            if (IsModifierPressed(key))
            {
                ExecuteHotkey(key);
            }
            else if (IsAlphanumeric(key))
            {
                ReadKeyIntoLine(key, lineBuilder, echoRead);
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                RemoveKeyFromLine(lineBuilder, echoRead);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                if (echoRead) Write();
            }
        }

        private static void RemoveKeyFromLine(StringBuilder lineBuilder, bool echoRead)
        {
            if (lineBuilder.Length > 0)
            {
                if (echoRead) Console.Write("\b \b");
                _textBuffer.Remove(_textBuffer.Length - 1, 1);
                lineBuilder.Remove(lineBuilder.Length - 1, 1);
            }
        }

        private static void ReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder lineBuilder, bool echoRead)
        {
            if (echoRead) WriteNoLine(key.KeyChar);
            lineBuilder.Append(key.KeyChar);
        }

        private static bool IsAlphanumeric(ConsoleKeyInfo key) => char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == ' ';

        private static bool IsModifierPressed(ConsoleKeyInfo key) =>
            key.Modifiers.HasFlag(ConsoleModifiers.Control)
            || key.Modifiers.HasFlag(ConsoleModifiers.Shift)
            || key.Modifiers.HasFlag(ConsoleModifiers.Alt);

        private static void ExecuteHotkey(ConsoleKeyInfo key)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control)
                && _controlHotkeys.TryGetValue(key.Key, out var action))
                ExecuteSubroutine(action);
        }
    }
}