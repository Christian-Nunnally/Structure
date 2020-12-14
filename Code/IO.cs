using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Structure
{
    public static class IO
    {
        private static readonly Stack<string> _textBuffers = new Stack<string>();
        private static readonly StringBuilder _textBuffer = new StringBuilder();
        private static bool _performingRegularActions = false;
        public static Dictionary<ConsoleKey, (Action Action, string Description)> Hotkeys { get; } = new Dictionary<ConsoleKey, (Action, string)>();

        public static void WriteHotkeys() => Hotkeys.All(x => Write($"ctrl + {$"{x.Key}".ToLower()}: {x.Value.Description}"));

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

        public static void PromptYesNo(string prompt, Action action) => PromptOptions(prompt, false, ('y', "yes", action), ('n', "no", null));

        public static void PromptOptions(string prompt, bool useDefault, params (char key, string description, Action action)[] options)
        {
            Write($"{prompt}\n");
            options.All(x => Write($"{x.key}: {x.description}"));
            ReadKey(PickOption);
            void PickOption(string result)
            {
                var (key, description, action) = options.FirstOrDefault(x => $"{x.key}" == result);
                if (useDefault)
                {
                    action ??= options.Last().action;
                }
                action?.Invoke();
            }
        }

        public static void Run(Action action) => Run(action, 0);

        public static void Run(Action action, int delay)
        {
            if (action is object)
            {
                _textBuffers.Push(_textBuffer.ToString());
                Clear();
                action();
                Thread.Sleep(delay * 1000);
                Clear();
                WriteNoLine(_textBuffers.Pop());
            }
        }

        private static void Read(Func<string, ConsoleKeyInfo, bool> shouldExit, Action<string> continuation, bool echo = true)
        {
            ConsoleKeyInfo key;
            var line = new StringBuilder();
            do
            {
                if (!_performingRegularActions)
                {
                    _performingRegularActions = true;
                    Program.RegularActions.All(x => x());
                    _performingRegularActions = false;
                }
                key = Console.ReadKey(true);
                ProcessReadKeyIntoLine(key, line, echo);
            } while (!shouldExit(line.ToString(), key));
            if (echo) Write();
            continuation(line.ToString());
        }

        private static void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo)
        {
            if (IsModifierPressed(key)) ExecuteHotkey(key);
            else if (IsAlphanumeric(key)) ReadKeyIntoLine(key, line, echo);
            else if (key.Key == ConsoleKey.Backspace) RemoveKeyFromLine(line, echo);
            else if (key.Key == ConsoleKey.Enter && echo) Write();
        }

        private static void RemoveKeyFromLine(StringBuilder line, bool echo)
        {
            if (line.Length > 0)
            {
                if (echo) Console.Write("\b \b");
                _textBuffer.Remove(_textBuffer.Length - 1, 1);
                line.Remove(line.Length - 1, 1);
            }
        }

        private static void ReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo)
        {
            if (echo) WriteNoLine(key.KeyChar);
            line.Append(key.KeyChar);
        }

        private static bool IsAlphanumeric(ConsoleKeyInfo key) => char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == ' ';

        private static bool IsModifierPressed(ConsoleKeyInfo key) =>
            key.Modifiers.HasFlag(ConsoleModifiers.Control)
            || key.Modifiers.HasFlag(ConsoleModifiers.Alt);

        private static void ExecuteHotkey(ConsoleKeyInfo key)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control)
                && Hotkeys.TryGetValue(key.Key, out var action))
                Run(action.Action);
        }
    }
}