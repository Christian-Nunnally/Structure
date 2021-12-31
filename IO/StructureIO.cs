using Structure.Code;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Structure
{
    public class StructureIO
    {
        private readonly List<string> _newsArchive = new List<string>();

        public IReadOnlyList<string> NewsArchive => _newsArchive;

        private IProgramInput _programInput;
        private IProgramOutput _programOutput;

        public CurrentTime CurrentTime { get; } = new CurrentTime();

        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly Queue<string> _newsQueue = new Queue<string>();
        private string _currentNews;
        private  int _newsCursorLeft = 40;

        public bool ThrowExceptions { get; set; }

        public event Action<ConsoleKeyInfo, StructureIO> InteruptKeyPressed;

        public void SetInput(IProgramInput input)
        {
            _programInput = input;
        }

        public void SetOutput(IProgramOutput output)
        {
            _programOutput = output;
        }

        public void Write(string text = "") => WriteNoLine($"{text}\n");

        public void WriteNoLine(string text = "")
        {
            _buffer.Append(text);
            _programOutput.Write(text);
        }

        public void Clear(bool clearConsole = true)
        {
            _buffer.Clear();
            if (clearConsole) _programOutput.Clear();
            _programOutput.SetCursorPosition(0, 1);
        }

        public void ReadAny() => Read((line, key) => true, x => { }, KeyGroups.NoKeys, echo: true);

        public void Read(Action<string> continuation, params ConsoleKey[] submitKeys) => Read((line, key) => submitKeys.Contains(key.Key), continuation, KeyGroups.NoKeys, echo: true);

        public void Read(Action<string> continuation) => Read(continuation, ConsoleKey.Enter);

        public void ReadInteger(string prompt, Action<int> continuation)
        {
            Write(prompt);
            Read(x =>
            {
                if (int.TryParse(x, out var integer))
                {
                    continuation(integer);
                }
                else
                {
                    Write($"'{x}' is not a valid integer.");
                    ReadInteger(prompt, continuation);
                }
            });
        }

        public void ReadKey(Action<string> continuation) => Read((line, key) => !IsModifierPressed(key), continuation, KeyGroups.MiscKeys, echo: false);

        public void PromptYesNo(string prompt, Action action) => PromptOptions(prompt, false, new UserAction("yes", action), new UserAction("no", null));

        public void PromptOptions(string prompt, bool useDefault, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            Write($"{prompt}\n");
            keyedOptions.All(x => Write($"{x.Key}: {x.Value.Description}"));
            ReadKey(PickOption);
            void PickOption(string result)
            {
                result = result.ToLower(CultureInfo.CurrentCulture);

                var (key, userAction) = keyedOptions.FirstOrDefault(x => $"{x.Key}" == result);
                var action = userAction?.Action;
                if (useDefault)
                {
                    action ??= options.Last().Action;
                }
                if (action is object)
                {
                    action();
                }
                else
                {
                    //throw new InvalidOperationException("Invalid input.");
                }
            }
        }

        public static (char Key, UserAction Action)[] CreateOptionKeys(UserAction[] options)
        {
            if (options == null) return null;
            var keys = new List<(char Key, UserAction Action)>();
            foreach (var option in options)
            {
                var possibleKeys = $"{option.Description.ToLower(CultureInfo.CurrentCulture)}abcdefghijklmnopqrstuvwxyz1234567890";
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

        public static Dictionary<char, UserAction> CreateOptionKeysDictionary(UserAction[] options)
        {
            return CreateOptionKeys(options).ToDictionary(x => x.Key, x=>x.Action);
        }

        public void News(string news)
        {
            if (!(_programOutput is NoOpOutput))
            {
                _newsQueue.Enqueue(news);
            }
            _newsArchive.Add(news);
        }

        public void Run(Action action)
        {
            if (action is null) return;
            _buffers.Push($"{_buffer}");
            Clear(true);
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (ThrowExceptions) throw new Exception("Exception" + e.Message, e);
                _programOutput.WriteLine(e.Message);
            }
            Clear(true);
            WriteNoLine(_buffers.Pop());
        }

        public void Refresh()
        {
            var buffer = _buffer.ToString();
            Clear(true);
            WriteNoLine(buffer);
        }

        private void Read(
            Func<string, ConsoleKeyInfo, bool> shouldExit, 
            Action<string> continuation, 
            ConsoleKey[] allowedKeys,
            bool echo)
        {
            ProgramInputData key;
            var line = new StringBuilder();
            do
            {
                while (!_programInput.IsKeyAvailable())
                {
                    if (!PrintNews()) break;
                    Thread.Sleep(10);
                }

                // TODO: Pass allowed keys in here all the time.
                key = _programInput.ReadKey();
                CurrentTime.SetArtificialTime(key.Time);
                ProcessReadKeyIntoLine(key.GetKeyInfo(), line, echo, allowedKeys);

            } while (!shouldExit(line.ToString(), key.GetKeyInfo()));
            if (echo) Write();
            continuation(line.ToString());
        }

        private bool PrintNews()
        {
            if (!_newsQueue.Any() && _currentNews == null) return false;
            _currentNews ??= _newsQueue.Dequeue();
            var cursorLeft = _programOutput.CursorLeft;
            var cursorTop = _programOutput.CursorTop;
            _programOutput.CursorLeft = Math.Max(0, _newsCursorLeft);
            _newsCursorLeft -= 2;
            _programOutput.CursorTop = 0;

            _programOutput.Write(_currentNews + "  ");

            _programOutput.CursorLeft = cursorLeft;
            _programOutput.CursorTop = cursorTop;
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
            return true;
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (IsModifierPressed(key))
            {
                InteruptKeyPressed?.Invoke(key, this);
            }
            else if (IsAlphanumeric(key) || key.Key == ConsoleKey.OemPeriod || key.Key == ConsoleKey.Decimal)
            {
                ReadStringIntoLine($"{key.KeyChar}", line, echo);
            }
            else if (allowedKeys.Contains(key.Key))
            {
                ReadStringIntoLine($"{{{key.Key}}}", line, echo);
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace) BackspaceFromLine(line, echo);
                else if (key.Key == ConsoleKey.Enter && echo) Write();
                else if (key.Key == ConsoleKey.Escape) Write();
            }
        }

        private void BackspaceFromLine(StringBuilder line, bool echo)
        {
            if (line.Length > 0)
            {
                if (echo)
                {
                    const string DoubleBackspace = "\b \b";
                    _programOutput.Write(DoubleBackspace);
                }

                _buffer.Remove(_buffer.Length - 1, 1);
                line.Remove(line.Length - 1, 1);
            }
        }

        private void ReadStringIntoLine(string text, StringBuilder line, bool echo)
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