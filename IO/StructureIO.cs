using Structure.IO.Input;
using Structure.IO.Output;
using Structure.Structure;
using Structure.Structure.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Structure.IO
{
    public class StructureIO
    {
        private const int X_START_POSITION = 0;
        private const int Y_START_POSITION = 1;
        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _currentBuffer = new StringBuilder();

        public List<IBackgroundProcess> BackgroundProcesses { get; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public Action<ConsoleKeyInfo, StructureIO> ModifierKeyAction { get; set; }

        public CurrentTime CurrentTime { get; }

        public bool SkipUnescesscaryOperations { get; set; }

        public StructureIO(StructureIoC ioc)
        {
            ModifierKeyAction = ioc.Get<Hotkey>().Execute;
            BackgroundProcesses = ioc?.GetAll<IBackgroundProcess>().ToList();
            CurrentTime = ioc.Get<CurrentTime>();
        }

        public void Refresh()
        {
            var buffer = _currentBuffer.ToString();
            Clear(true);
            WriteNoLine(buffer);
        }

        public void ClearStaleOutput()
        {
            using var savePosition = new SaveAndRestoreCursorPosition(ProgramOutput);
            var buffer = _currentBuffer.ToString();
            var x = X_START_POSITION;
            var y = Y_START_POSITION;
            foreach (var character in buffer)
            {
                ProgramOutput.SetCursorPosition(x++, y);
                if (!char.IsWhiteSpace(character)) continue;
                
                if (character == '\n' || x >= 80)
                {
                    while (x++ < ProgramOutput.Width) ProgramOutput.Write(" ");
                    y++;
                    x = 0;
                }
                else ProgramOutput.Write(" ");
            }
            for (int i = 0; i < 20; i++) ProgramOutput.Write("                                                                                                  ");
        }

        public void Clear(bool clearConsole)
        {
            _currentBuffer.Clear();
            if (clearConsole) ProgramOutput.Clear();
            ProgramOutput.SetCursorPosition(0, 1);
        }

        public void Run(Action action)
        {
            _buffers.Push($"{_currentBuffer}");
            Clear(true);
            Executor.SafelyExecute(action);
            Clear(true);
            WriteNoLine(_buffers.Pop());
        }

        public void Write(string text = "") => WriteNoLine($"{text}\n");

        public void WriteNoLine(string text)
        {
            _currentBuffer.Append(text);
            ProgramOutput.Write(text);
        }

        public void Read(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys)
            => ReadCore(continuation, allowedKeys, submitKeys, allowedKeys);

        public void ReadCore(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys, ConsoleKey[] allowedReadKey)
        {
            var line = new StringBuilder();
            while (true)
            {
                var key = ReadKey(allowedReadKey);
                ProcessReadKeyIntoLine(key, line, true, allowedKeys);
                if (submitKeys.Contains(key.Key) || submitKeys == KeyGroups.NoKeys) break;
            }
            Write();
            continuation?.Invoke(line.ToString());
        }

        public void ReadInteger(string prompt, Action<int> continuation)
        {
            void continueWhenInteger(string x)
            {
                if (int.TryParse(x, out var integer)) continuation(integer);
                else
                {
                    Write($"'{x}' is not a valid integer.");
                    Run(() => ReadInteger(prompt, continuation));
                }
            }
            Write(prompt);
            Read(continueWhenInteger, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
        }

        public void ReadOptions(string prompt, params UserAction[] options) => ReadOptions(prompt, null, options);

        public void ReadOptions(string prompt, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            var possibleKeys = keyedOptions.Select(x => x.Key.Key).ToArray();
            ReadOptionsCore(prompt, helpString, possibleKeys, keyedOptions);
        }

        public ConsoleKeyInfo ReadKey(ConsoleKey[] allowedKeys)
        {
            while (true)
            {
                ProcessInBackgroundWhileWaitingForInput();
                var keyInfo = ReadKeyAndSetTime();
                var isHotkeyPressed = ConsoleKeyHelpers.IsModifierPressed(keyInfo);
                var isAllowedKey = allowedKeys.Contains(keyInfo.Key) || allowedKeys == KeyGroups.NoKeys;
                if (isHotkeyPressed) ModifierKeyAction?.Invoke(keyInfo, this);
                else if (isAllowedKey) return keyInfo;
                else ProgramInput.RemoveLastReadKey();
                return ReadKey(allowedKeys);
            }
        }

        private void ReadOptionsCore(string prompt, string helpString, ConsoleKey[] possibleKeys, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            PrintOptions(prompt, helpString, keyedOptions);
            ReadKeyAndSelectOption(keyedOptions, possibleKeys);
        }

        private void PrintOptions(string prompt, string helpString, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            Write($"{prompt}\n");
            if (string.IsNullOrEmpty(helpString)) keyedOptions.All(PrintOption);
            else Write(helpString);
        }

        private void PrintOption(KeyValuePair<ConsoleKeyInfo, UserAction> x) => WriteNoLine(OptionString(x.Key, x.Value));

        private static string OptionString(ConsoleKeyInfo key, UserAction option) => !string.IsNullOrEmpty(option.Description) ? $" {Utility.KeyToKeyString(key)} - {option.Description}\n" : string.Empty;

        private void ReadKeyAndSelectOption(Dictionary<ConsoleKeyInfo, UserAction> keyedOptions, ConsoleKey[] possibleKeys)
        {
            var key = ReadKey(possibleKeys);
            var exactMatchExists = keyedOptions.Any(x => x.Key.Key == key.Key);
            var match = keyedOptions.FirstOrDefault(x => x.Key.Key == key.Key);
            if (exactMatchExists)
                match.Value.Action();
            else if (int.TryParse($"{key.KeyChar}", out var _) && keyedOptions.Any(x => x.Key.KeyChar == key.KeyChar))
                keyedOptions.First(x => x.Key.KeyChar == key.KeyChar).Value.Action();
        }

        private ConsoleKeyInfo ReadKeyAndSetTime()
        {
            var key = ProgramInput.ReadKey();
            if (key == null) throw new InvalidProgramException();
            CurrentTime.SetArtificialTime(key.Time);
            return key.GetKeyInfo();
        }

        private void ProcessInBackgroundWhileWaitingForInput()
        {
            bool isNoInputAndBackgroundprocessesWorking() => !ProgramInput.IsKeyAvailable() && BackgroundProcesses.Any(x => x.DoProcess(this));
            while (isNoInputAndBackgroundprocessesWorking()) Thread.Sleep(10);
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (ConsoleKeyHelpers.IsAlphanumeric(key))
            {
                ReadStringIntoLine($"{key.KeyChar}", line, echo);
            }
            else if (allowedKeys.Contains(key.Key))
            {
                if (key.Key == ConsoleKey.Backspace) BackspaceFromLine(line, echo);
                else if (key.Key == ConsoleKey.Enter && echo) Write();
                else ReadStringIntoLine($"{{{key.Key}}}", line, echo);
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

        public void ReadOptionsObsolete(string prompt, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            ReadOptionsCore(prompt, helpString, KeyGroups.NoKeys, keyedOptions);
        }

        private static Dictionary<ConsoleKeyInfo, UserAction> CreateOptionKeysDictionary(UserAction[] options)
        {
            var keys = new List<(ConsoleKeyInfo Key, UserAction Action)>();
            options.Where(x => x.HotkeyOverridden).All(x => keys.Add((x.Hotkey, x)));
            foreach (var option in options.Where(x => !x.HotkeyOverridden))
            {
                var possibleKeys = $"{option.Description.ToLower(CultureInfo.CurrentCulture)}abcdefghijklmnopqrstuvwxyz1234567890";
                for (int i = 0; i < possibleKeys.Length; i++)
                {
                    if (char.IsWhiteSpace(possibleKeys[i])) continue;
                    if (!keys.Any(x => x.Key.KeyChar == ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]).KeyChar))
                    {
                        var consoleKeyInfo = ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]);
                        if (consoleKeyInfo.Key == ConsoleKey.NumPad0) keys.Add((new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false), option));
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