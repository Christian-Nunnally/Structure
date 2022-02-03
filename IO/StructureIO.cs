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
        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _currentBuffer = new StringBuilder();
        private readonly NewsPrinter _newsPrinter;

        public CurrentTime CurrentTime { get; } = new CurrentTime();

        public Hotkey Hotkey { get; private set; }

        public bool ThrowExceptions { get; set; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public StructureIO(Hotkey hotkey, NewsPrinter newsPrinter)
        {
            Hotkey = hotkey;
            _newsPrinter = newsPrinter;
        }

        public void Write(string text = "") => WriteNoLine($"{text}\n");

        public void WriteNoLine(string text)
        {
            _currentBuffer.Append(text);
            ProgramOutput.Write(text);
        }

        public void Read(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys, bool echo = true)
        {
            var line = new StringBuilder();
            while (true)
            {
                var key = ReadKey(allowedKeys.Concat(submitKeys).ToArray());
                ProcessReadKeyIntoLine(key, line, echo, allowedKeys);

                if (submitKeys.Contains(key.Key)) break;
                if (submitKeys == KeyGroups.NoKeys) break;
            }
            if (echo) Write();
            continuation?.Invoke(line.ToString());
        }

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
            }, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
        }

        public void PromptOptions(string prompt, bool useDefault, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            Write($"{prompt}\n");
            if (string.IsNullOrEmpty(helpString)) keyedOptions.All(x => Write($" {Utility.KeyToKeyString(x.Key)} - {x.Value.Description}"));
            else Write(helpString);

            ConsoleKeyInfo key;
            
            key = ReadKey(KeyGroups.NoKeys);
            if (char.IsUpper(key.KeyChar))
            {
                if (useDefault) options.Last().Action();
                return;
            }
            var exactMatchExists = keyedOptions.Any(x => x.Key.Key == key.Key);
            var match = keyedOptions.FirstOrDefault(x => x.Key.Key == key.Key);
            if (useDefault && !exactMatchExists)
            {
                options.Last().Action();
            }
            else if (exactMatchExists)
            {
                match.Value.Action();
            }
            else if (int.TryParse($"{key.KeyChar}", out var _) && keyedOptions.Any(x => x.Key.KeyChar == key.KeyChar))
            {
                var selectedNumericOption = keyedOptions.First(x => x.Key.KeyChar == key.KeyChar);
                selectedNumericOption.Value.Action();
            }
        }

        public void News(string news)
        {
            _newsPrinter.EnqueueNews(news);
        }

        public void Clear(bool clearConsole)
        {
            _currentBuffer.Clear();
            if (clearConsole) ProgramOutput.Clear();
            ProgramOutput.SetCursorPosition(0, 1);
        }

        public static Dictionary<ConsoleKeyInfo, UserAction> CreateOptionKeysDictionary(UserAction[] options)
        {
            if (options == null) return null;
            var keys = new List<(ConsoleKeyInfo Key, UserAction Action)>();
            foreach (var option in options)
            {
                if (option.HotkeyOverridden)
                {
                    keys.Add((option.Hotkey, option));
                    continue;
                }

                var possibleKeys = $"{option.Description.ToLower(CultureInfo.CurrentCulture)}abcdefghijklmnopqrstuvwxyz1234567890";
                for (int i = 0; i < possibleKeys.Length; i++)
                {
                    if (char.IsWhiteSpace(possibleKeys[i])) continue;
                    if (!keys.Any(x => x.Key.KeyChar == ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]).KeyChar))
                    {
                        keys.Add((ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]), option));
                        break;
                    }
                }
            }
            return keys.ToDictionary(x => x.Key, x => x.Action);
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
                Write(e.Message);
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

        public ConsoleKeyInfo ReadKey(ConsoleKey[] allowedKeys)
        {
            while (true)
            {
                PrintNewsWhileWaitingForInput();
                var keyInfo = ReadKeyAndSetTime();
                var wasHotkeyPressed = ConsoleKeyHelpers.IsModifierPressed(keyInfo);
                if (wasHotkeyPressed) Hotkey.Execute(keyInfo, this);
                else if (allowedKeys.Contains(keyInfo.Key))
                {
                    return keyInfo;
                }
                //TODO: Temp
                else if (allowedKeys == KeyGroups.MiscKeys)
                {
                    News($"{keyInfo.KeyChar} relying on misc keys hack.");
                    return keyInfo;
                }
                else
                {
                    News($"{keyInfo.KeyChar} is not a currently recognized input.");
                    // throw new InvalidOperationException("Not allowed key");
                    return keyInfo;
                }
            }
        }

        private ConsoleKeyInfo ReadKeyAndSetTime()
        {
            var key = ProgramInput.ReadKey();
            CurrentTime.SetArtificialTime(key.Time);
            return key.GetKeyInfo();
        }

        private void PrintNewsWhileWaitingForInput()
        {
            bool isNoInputAndNewsStillPrinting() => !ProgramInput.IsKeyAvailable() && _newsPrinter.PrintNews(ProgramOutput);
            while (isNoInputAndNewsStillPrinting()) Thread.Sleep(10);
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (ConsoleKeyHelpers.IsAlphanumeric(key))
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
            if (line.Length == 0) return;
            if (echo) ProgramOutput.Write("\b \b");
            _currentBuffer.Remove(_currentBuffer.Length - 1, 1);
            line.Remove(line.Length - 1, 1);
        }

        private void ReadStringIntoLine(string text, StringBuilder line, bool echo)
        {
            if (echo) WriteNoLine(text);
            line.Append(text);
        }
    }
}