﻿using Structure.IO.Input;
using Structure.IO.Output;
using Structure.Structure;
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
        private readonly Stack<string> _buffers = new Stack<string>();
        private readonly StringBuilder _currentBuffer = new StringBuilder();

        public List<IBackgroundProcess> BackgroundProcesses { get; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public Action<ConsoleKeyInfo, StructureIO> ModifierKeyAction { get; set; } 

        public CurrentTime CurrentTime { get; }

        public bool ThrowExceptions { get; set; }

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
            SafelyExecute(action);
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
                if (!int.TryParse(x, out var integer))
                {
                    Write($"'{x}' is not a valid integer.");
                    Run(() => ReadInteger(prompt, continuation));
                    return;
                }
                continuation(integer);
            }
            Write(prompt);
            Read(continueWhenInteger, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
        }

        public void ReadOptions(string prompt, bool useDefault, params UserAction[] options) => ReadOptions(prompt, useDefault, null, options);

        public void ReadOptions(string prompt, bool useDefault, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            var possibleKeys = keyedOptions.Select(x => x.Key.Key).ToArray();
            if (useDefault) possibleKeys = KeyGroups.NoKeys;
            ReadOptionsCore(prompt, useDefault, helpString, options, possibleKeys, keyedOptions);
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

        // TODO: probs remove.
        private static Dictionary<ConsoleKeyInfo, UserAction> CreateOptionKeysDictionary(UserAction[] options)
        {
            if (options == null) return null;
            var keys = new List<(ConsoleKeyInfo Key, UserAction Action)>();
            foreach (var option in options.Where(x => x.HotkeyOverridden))
            {
                keys.Add((option.Hotkey, option));
            }
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

        private void ReadOptionsCore(string prompt, bool useDefault, string helpString, UserAction[] options, ConsoleKey[] possibleKeys, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            PrintOptions(prompt, helpString, keyedOptions);
            ReadKeyAndSelectOption(useDefault, options.Last(), keyedOptions, possibleKeys);
        }

        private void PrintOptions(string prompt, string helpString, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            Write($"{prompt}\n");
            if (string.IsNullOrEmpty(helpString)) keyedOptions.All(x =>
            {
                if (!string.IsNullOrEmpty(x.Value.Description)) Write($" {Utility.KeyToKeyString(x.Key)} - {x.Value.Description}");
            });
            else Write(helpString);
        }

        private void ReadKeyAndSelectOption(bool useDefault, UserAction defaultAction, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions, ConsoleKey[] possibleKeys)
        {
            var key = ReadKey(possibleKeys);
            if (char.IsUpper(key.KeyChar) && useDefault) defaultAction.Action();
            if (char.IsUpper(key.KeyChar)) return;
            var exactMatchExists = keyedOptions.Any(x => x.Key.Key == key.Key);
            var match = keyedOptions.FirstOrDefault(x => x.Key.Key == key.Key);
            if (useDefault && !exactMatchExists)
                defaultAction.Action();
            else if (exactMatchExists)
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

        private void SafelyExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (ThrowExceptions) throw new Exception("Exception" + e.Message, e);
            }
        }

        public void ReadObsolete(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys)
        {
            var allowedReadKey = KeyGroups.NoKeys;
            ReadCore(continuation, allowedKeys, submitKeys, allowedReadKey);
        }

        public void ReadOptionsObsolete(string prompt, bool useDefault, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            ReadOptionsCore(prompt, useDefault, helpString, options, KeyGroups.NoKeys, keyedOptions);
        }
    }
}