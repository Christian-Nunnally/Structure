using Structure.Code;
using Structure.IO;
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
        public CurrentTime CurrentTime { get; } = new CurrentTime();

        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _currentBuffer = new StringBuilder();
        private readonly NewsPrinter _newsPrinter = new NewsPrinter();
        private IProgramOutput programOutput;
        public Hotkey Hotkey { get; private set; }

        public bool ThrowExceptions { get; set; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput 
        { 
            get => programOutput; 
            set
            {
                programOutput = value;
                // TODO: Maybe handle this less magically?
                _newsPrinter.ClearNews();
            }
        }

        public StructureIO(Hotkey hotkey)
        {
            Hotkey = hotkey;
        }

        public void Write(string text = "") => WriteNoLine($"{text}\n");

        public void WriteNoLine(string text = "")
        {
            _currentBuffer.Append(text);
            ProgramOutput.Write(text);
        }

        public void Clear(bool clearConsole = true)
        {
            _currentBuffer.Clear();
            if (clearConsole) ProgramOutput.Clear();
            ProgramOutput.SetCursorPosition(0, 1);
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
            return CreateOptionKeys(options).ToDictionary(x => x.Key, x => x.Action);
        }

        public void News(string news)
        {
            _newsPrinter.EnqueueNews(news);
        }

        public void Run(Action action)
        {
            if (action is null) return;
            _buffers.Push($"{_currentBuffer}");
            Clear(true);
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (ThrowExceptions) throw new Exception("Exception" + e.Message, e);
                ProgramOutput.WriteLine(e.Message);
            }
            Clear(true);
            WriteNoLine(_buffers.Pop());
        }

        public void Refresh()
        {
            var buffer = _currentBuffer.ToString();
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
                while (!ProgramInput.IsKeyAvailable())
                {
                    if (!_newsPrinter.PrintNews(ProgramOutput)) break;
                    Thread.Sleep(10);
                }

                // TODO: Pass allowed keys in here all the time.
                key = ProgramInput.ReadKey();
                CurrentTime.SetArtificialTime(key.Time);
                ProcessReadKeyIntoLine(key.GetKeyInfo(), line, echo, allowedKeys);

            } while (!shouldExit(line.ToString(), key.GetKeyInfo()));
            if (echo) Write();
            continuation(line.ToString());
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (IsModifierPressed(key))
            {
                Hotkey.Execute(key, this);
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
                    ProgramOutput.Write(DoubleBackspace);
                }

                _currentBuffer.Remove(_currentBuffer.Length - 1, 1);
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