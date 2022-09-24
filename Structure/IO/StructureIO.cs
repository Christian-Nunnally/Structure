using Structur.IO.Input;
using Structur.IO.Output;
using Structur.IO.Persistence;
using Structur.Program;
using Structur.Program.Utilities;
using Structure.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Structur.IO
{
    public class StructureIO
    {
        private readonly Stack<string> _buffers = new();
        private readonly List<IBackgroundProcess> _backgroundProcesses;
        private readonly OptionHelpPrinter _optionHelpPrinter;

        public int XStartPosition { get; set; }

        public int YStartPosition { get; set; } = 1;

        public bool IsBusy { get; set; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public StringBuilder CurrentBuffer { get; } = new StringBuilder();

        public string CurrentDisplay => CurrentBuffer.ToString();

        public Action<ConsoleKeyInfo, StructureIO> ModifierKeyAction { get; set; }

        public CurrentTime CurrentTime { get; }

        public bool SkipUnescesscaryOperations { get; set; }

        public int KeyCount { get; private set; }

        public int KeyHash { get; private set; }

        public StructureIO(StructureIoC ioc)
        {
            ModifierKeyAction = ioc.Get<Hotkey>().Execute;
            _backgroundProcesses = ioc?.GetAll<IBackgroundProcess>().ToList();
            CurrentTime = ioc.Get<CurrentTime>();
            _optionHelpPrinter = new OptionHelpPrinter(this);
        }

        public void ClearStaleOutput()
        {
            _backgroundProcesses.OfType<StaleOutputClearer>().FirstOrDefault()?.ClearStaleOutput();
        }

        public void ClearBuffer()
        {
            CurrentBuffer.Clear();
            ProgramOutput.CursorTop = YStartPosition;
            ProgramOutput.CursorLeft = XStartPosition;
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
                // TODO: This might be breaking change.
                if (key.Key == ConsoleKey.Escape) return;
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

        private void IncrementKeyHash(ConsoleKeyInfo key)
        {
            KeyCount++;
            KeyHash = GenerateNextInputHash(KeyHash, key);
        }

        public static int GenerateNextInputHash(int hash, ConsoleKeyInfo key) => (hash + 7) * (int)key.Key % 27277;

        public ConsoleKeyInfo ReadKey(ConsoleKey[] allowedKeys)
        {
            while (true)
            {
                if (!SkipUnescesscaryOperations) ProcessInBackgroundWhileWaitingForInput();
                var keyInfo = ReadKeyAndSetTime();
                var isHotkeyPressed = ConsoleKeyHelpers.IsModifierPressed(keyInfo);
                var isAllowedKey = allowedKeys.Contains(keyInfo.Key) || allowedKeys == KeyGroups.NoKeys;
                if (isHotkeyPressed)
                {
                    IncrementKeyHash(keyInfo);
                    ModifierKeyAction?.Invoke(keyInfo, this);
                }
                else if (isAllowedKey)
                {
                    IncrementKeyHash(keyInfo);
                    return keyInfo;
                }
                else ProgramInput.RemoveLastInput();
                return ReadKey(allowedKeys);
            }
        }

        private void ReadOptionsCore(string prompt, string helpString, ConsoleKey[] possibleKeys, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            Write($"{prompt}\n");
            _optionHelpPrinter.PrintHelp(helpString, keyedOptions);
            ReadKeyAndSelectOption(keyedOptions, possibleKeys);
        }

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
            IsBusy = false;
            var key = ProgramInput.ReadInput();
            IsBusy = true;
            if (key == null) throw new InvalidProgramException();
            CurrentTime.SetArtificialTime(key.Time);
            return key.GetKeyInfo();
        }

        public void ProcessInBackgroundWhileWaitingForInput()
        {
            while (!ProgramInput.IsInputAvailable() && ProcessBackgroundWork());
        }

        public void ProcessAllBackgroundWork()
        {
            while (ProcessBackgroundWork());
        }

        private bool ProcessBackgroundWork() => _backgroundProcesses.Any(x => x.DoProcess(this));

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
                if (allowedKeys == KeyGroups.NoKeys) return;
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