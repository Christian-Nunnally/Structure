using Structure.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Structure
{
    public class StructureIO
    {
        private List<string> _newsArchive = new List<string>();
        public IReadOnlyList<string> NewsArchive => _newsArchive;

        private IProgramInput _programInput;
        private IProgramOutput _programOutput;

        public CurrentTime CurrentTime { get; } = new CurrentTime();

        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly Queue<string> _newsQueue = new Queue<string>();
        private string _currentNews;
        private  int _newsCursorLeft = 40;
        private readonly Hotkey _hotkey;

        public bool ThrowExceptions { get; set; }

        public StructureIO(Hotkey hotkey)
        {
            _hotkey = hotkey;
        }

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
                result = result.ToLower();

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

        public (char Key, UserAction Action)[] CreateOptionKeys(UserAction[] options)
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

        public Dictionary<char, UserAction> CreateOptionKeysDictionary(UserAction[] options)
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
                _hotkey.Execute(key, this);
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

        //private char GetCharFromKey(ConsoleKeyInfo key)
        //{
        //    if (key.Key == ConsoleKey.D0) return '0';
        //    if (key.Key == ConsoleKey.D1) return '1';
        //    if (key.Key == ConsoleKey.D2) return '2';
        //    if (key.Key == ConsoleKey.D3) return '3';
        //    if (key.Key == ConsoleKey.D4) return '4';
        //    if (key.Key == ConsoleKey.D5) return '5';
        //    if (key.Key == ConsoleKey.D6) return '6';
        //    if (key.Key == ConsoleKey.D7) return '7';
        //    if (key.Key == ConsoleKey.D8) return '8';
        //    if (key.Key == ConsoleKey.D9) return '9';
        //    return key.KeyChar;
        //}

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

        private bool IsAlphanumeric(ConsoleKeyInfo key) => char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == ' ';

        private bool IsModifierPressed(ConsoleKeyInfo key) =>
            key.Modifiers.HasFlag(ConsoleModifiers.Control)
            || key.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}