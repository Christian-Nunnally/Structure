using Structure.IO.Input;
using Structure.IO.Output;
using Structure.Structure;
using Structure.Structure.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Structure.IO
{
    public class StructureIO
    {
        private const int X_START_POSITION = 0;
        private const int Y_START_POSITION = 1;
        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly List<IBackgroundProcess> _backgroundProcesses;

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public StringBuilder CurrentBuffer { get; } = new StringBuilder();

        public Action<ConsoleKeyInfo, StructureIO> ModifierKeyAction { get; set; }

        public CurrentTime CurrentTime { get; }

        public bool SkipUnescesscaryOperations { get; set; }

        public StructureIO(StructureIoC ioc)
        {
            ModifierKeyAction = ioc.Get<Hotkey>().Execute;
            _backgroundProcesses = ioc?.GetAll<IBackgroundProcess>().ToList();
            CurrentTime = ioc.Get<CurrentTime>();
        }

        public void ClearStaleOutput()
        {
            _backgroundProcesses.OfType<StaleOutputClearer>().FirstOrDefault()?.ClearStaleOutput(this);
        }

        public void ClearBuffer()
        {
            CurrentBuffer.Clear();
            ProgramOutput.SetCursorPosition(X_START_POSITION, Y_START_POSITION);
        }

        public void Run(Action action)
        {
            _buffers.Push($"{CurrentBuffer}");
            ProgramOutput.Clear();
            ClearBuffer();
            Executor.SafelyExecute(action);
            ClearBuffer();
            WriteNoLine(_buffers.Pop());
            ClearStaleOutput();
        }

        public void Write(string text = "") => WriteNoLine($"{text}\n");

        public void WriteNoLine(string text)
        {
            CurrentBuffer.Append(text);
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

        public void ReadOptions(string prompt, string customHelpString, params UserAction[] options)
        {
            var keyedOptions = ConsoleKeyHelpers.CreateUserActionToConsoleKeyMap(options);
            var possibleKeys = keyedOptions.Select(x => x.Key.Key).ToArray();
            ReadOptionsCore(prompt, customHelpString, possibleKeys, keyedOptions);
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
            while (!ProgramInput.IsKeyAvailable() && _backgroundProcesses.Any(x => x.DoProcess(this)));
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (ConsoleKeyHelpers.IsAlphanumeric(key))
            {
                AppendStringToLine($"{key.KeyChar}", line, echo);
            }
            else if (allowedKeys.Contains(key.Key))
            {
                if (key.Key == ConsoleKey.Backspace) BackspaceKeyFromLine(line, echo);
                else if (key.Key == ConsoleKey.Enter && echo) Write();
                else AppendStringToLine($"{{{key.Key}}}", line, echo);
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace) BackspaceKeyFromLine(line, echo);
                else if (key.Key == ConsoleKey.Enter && echo) Write();
                else if (key.Key == ConsoleKey.Escape) Write();
            }
        }

        private void BackspaceKeyFromLine(StringBuilder line, bool echo)
        {
            if (line.Length == 0) return;
            if (echo) ProgramOutput.Write("\b \b");
            CurrentBuffer.Remove(CurrentBuffer.Length - 1, 1);
            line.Remove(line.Length - 1, 1);
        }

        private void AppendStringToLine(string text, StringBuilder line, bool echo)
        {
            if (echo) WriteNoLine(text);
            line.Append(text);
        }

        public void ReadOptionsObsolete(string prompt, string helpString, params UserAction[] options)
        {
            var keyedOptions = ConsoleKeyHelpers.CreateUserActionToConsoleKeyMap(options);
            ReadOptionsCore(prompt, helpString, KeyGroups.NoKeys, keyedOptions);
        }
    }
}