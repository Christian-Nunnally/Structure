using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Structure
{
    public static class IO
    {
        public static PersistedList<string> NewsArchive = new PersistedList<string>("NewsArchive");

        private static Stack<string> _buffers = new Stack<string>();
        private static StringBuilder _buffer = new StringBuilder();
        private static Queue<string> _newsQueue = new Queue<string>();
        private static bool _doingActions = false;
        private static string _currentNews;
        private static int _newsCursorLeft = 40;

        public static void Write(string text = "") => WriteNoLine($"{text}\n");

        public static void WriteNoLine(string text = "")
        {
            _buffer.Append(text);
            Console.Write(text);
        }

        public static void Clear(bool clearConsole = true)
        {
            _buffer.Clear();
            if (clearConsole) Console.Clear();
            Console.SetCursorPosition(0, 1);
        }

        public static void ReadAny() => Read((line, key) => true, x => { }, false);

        public static void Read(Action<string> continuation, params ConsoleKey[] submitKeys) => Read((line, key) => submitKeys.Contains(key.Key), continuation, false);

        public static void Read(Action<string> continuation) => Read(continuation, ConsoleKey.Enter);

        public static void ReadKey(Action<string> continuation) => Read((line, key) => !IsModifierPressed(key), continuation, true, false);

        public static void PromptYesNo(string prompt, Action action) => PromptOptions(prompt, false, new UserAction("yes", action), new UserAction("no", null));

        public static void PromptOptions(string prompt, bool useDefault, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeys(options);
            Write($"{prompt}\n");
            keyedOptions.All(x => Write($"{x.Key}: {x.Action.Description}"));
            ReadKey(PickOption);
            void PickOption(string result)
            {
                result = result.ToLower();
                var (key, userAction) = keyedOptions.FirstOrDefault(x => $"{x.Key}" == result);
                var action = userAction?.Action;
                if (useDefault)
                {
                    action ??= options.Last().Action;
                }
                action?.Invoke();
            }
        }

        public static (char Key, UserAction Action)[] CreateOptionKeys(UserAction[] options)
        {
            var keys = new List<(char Key, UserAction Action)>();
            foreach (var option in options)
            {
                var possibleKeys = $"{option.Description.ToLower()}abcdefghijklmnopqrstuvwxyz1234567890";
                for (int i = 0; i < possibleKeys.Length; i++)
                {
                    if (!keys.Any(x => x.Key == possibleKeys[i]))
                    {
                        keys.Add((possibleKeys[i], option));
                        break;
                    }
                }
            }
            return keys.ToArray();
        }

        public static void News(string news)
        {
            _newsQueue.Enqueue(news);
            NewsArchive.Add(news);
        }

        public static void Run(Action action)
        {
            if (action is null) return;
            _buffers.Push($"{_buffer}");
            Clear(true);
            action();
            Clear(true);
            WriteNoLine(_buffers.Pop());
        }

        private static void Read(Func<string, ConsoleKeyInfo, bool> shouldExit, Action<string> continuation, bool allowMiscKeys, bool echo = true)
        {
            ConsoleKeyInfo key;
            var line = new StringBuilder();
            do
            {
                if (!_doingActions)
                {
                    _doingActions = true;
                    Program.RegularActions.All(x => x());
                    _doingActions = false;
                }

                while (!Console.KeyAvailable)
                {
                    PrintNews();
                    Thread.Sleep(10);
                }

                key = Console.ReadKey(true);
                ProcessReadKeyIntoLine(key, line, allowMiscKeys, echo);
            } while (!shouldExit(line.ToString(), key));
            if (echo) Write();
            continuation(line.ToString());
        }

        private static void PrintNews()
        {
            if (!_newsQueue.Any() && _currentNews == null) return;
            _currentNews ??= _newsQueue.Dequeue();
            var cursorLeft = Console.CursorLeft;
            var cursorTop = Console.CursorTop;
            Console.CursorLeft = Math.Max(0, _newsCursorLeft);
            _newsCursorLeft -= 2;
            Console.CursorTop = 0;

            Console.Write(_currentNews + "  ");

            Console.CursorLeft = cursorLeft;
            Console.CursorTop = cursorTop;
            if (_newsCursorLeft < -80)
            {
                if (_currentNews.Length == 0)
                {
                    _currentNews = null;
                    _newsCursorLeft = 40;
                }
                else if (_currentNews.Length == 1) _currentNews = _currentNews[1..];
                else _currentNews = _currentNews[2..];
            }
        }

        private static void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool allowMiscKeys, bool echo)
        {
            if (IsModifierPressed(key))
            {
                Hotkey.Execute(key);
                return;
            }
            if (IsAlphanumeric(key)) ReadKeyIntoLine(key, line, echo);
            else if (allowMiscKeys)
            {
                var allowedKeys = new[] { ConsoleKey.Enter, ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.Delete, ConsoleKey.Escape };
                if (allowedKeys.Contains(key.Key)) ReadStringIntoLine($"{{{key.Key}}}", line, echo);
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace) BackspaceFromLine(line, echo);
                else if (key.Key == ConsoleKey.Enter && echo) Write();
            }
        }

        private static void BackspaceFromLine(StringBuilder line, bool echo)
        {
            if (line.Length > 0)
            {
                if (echo) Console.Write("\b \b");
                _buffer.Remove(_buffer.Length - 1, 1);
                line.Remove(line.Length - 1, 1);
            }
        }

        private static void ReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo) => ReadStringIntoLine($"{key.KeyChar}", line, echo);

        private static void ReadStringIntoLine(string text, StringBuilder line, bool echo)
        {
            if (echo) WriteNoLine(text);
            line.Append(text);
        }

        private static bool IsAlphanumeric(ConsoleKeyInfo key) => char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == ' ';

        private static bool IsModifierPressed(ConsoleKeyInfo key) =>
            key.Modifiers.HasFlag(ConsoleModifiers.Control)
            || key.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}