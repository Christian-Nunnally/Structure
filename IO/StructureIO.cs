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
        private readonly INewsPrinter _newsPrinter;

        public CurrentTime CurrentTime { get; } = new CurrentTime();

        public Hotkey Hotkey { get; private set; }

        public bool ThrowExceptions { get; set; }

        public IProgramInput ProgramInput { get; set; }

        public IProgramOutput ProgramOutput { get; set; }

        public StructureIO(Hotkey hotkey, INewsPrinter newsPrinter)
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

        public void ReadObsolete(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys, bool echo = true)
        {
            var allowedReadKey = KeyGroups.NoKeys;
            ReadCore(continuation, allowedKeys, submitKeys, echo, allowedReadKey);
        }

        public void Read(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys, bool echo = true)
        {
            ReadCore(continuation, allowedKeys, submitKeys, echo, allowedKeys);
        }

        private void ReadCore(Action<string> continuation, ConsoleKey[] allowedKeys, ConsoleKey[] submitKeys, bool echo, ConsoleKey[] allowedReadKey)
        {
            var line = new StringBuilder();
            while (true)
            {
                var key = ReadKey(allowedReadKey);
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

        public void PromptOptions(string prompt, bool useDefault, params UserAction[] options) => PromptOptions(prompt, useDefault, null, options);

        public void PromptOptions(string prompt, bool useDefault, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            var possibleKeys = keyedOptions.Select(x => x.Key.Key).ToArray();
            if (useDefault) possibleKeys = KeyGroups.NoKeys;
            PromptOptionsCore(prompt, useDefault, helpString, options, possibleKeys, keyedOptions);
        }

        public void PromptOptionsObsolete(string prompt, bool useDefault, string helpString, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            PromptOptionsCore(prompt, useDefault, helpString, options, KeyGroups.NoKeys, keyedOptions);
        }

        private void PromptOptionsCore(string prompt, bool useDefault, string helpString, UserAction[] options, ConsoleKey[] possibleKeys, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            PrintOptions(prompt, helpString, keyedOptions);
            ReadKeyAndSelectOption(useDefault, options.Last(), keyedOptions, possibleKeys);
        }

        private void PrintOptions(string prompt, string helpString, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            Write($"{prompt}\n");
            if (string.IsNullOrEmpty(helpString)) keyedOptions.All(x => Write($" {Utility.KeyToKeyString(x.Key)} - {x.Value.Description}"));
            else Write(helpString);
        }

        private void ReadKeyAndSelectOption(bool useDefault, UserAction defaultAction, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions, ConsoleKey[] possibleKeys)
        {
            var key = ReadKey(possibleKeys);
            if (char.IsUpper(key.KeyChar))
            {
                if (useDefault) defaultAction.Action();
                return;
            }
            var exactMatchExists = keyedOptions.Any(x => x.Key.Key == key.Key);
            var match = keyedOptions.FirstOrDefault(x => x.Key.Key == key.Key);
            if (useDefault && !exactMatchExists)
            {
                defaultAction.Action();
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

        public void SubmitNews(string news)
        {
            _newsPrinter?.EnqueueNews(news);
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
                        var consoleKeyInfo = ConsoleKeyHelpers.ConvertCharToConsoleKey(possibleKeys[i]);
                        keys.Add((consoleKeyInfo, option));
                        option.Hotkey = consoleKeyInfo;
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
            catch
            {
                int pleaseHlp = 0;
                //if (ThrowExceptions) throw new Exception("Exception" + e.Message, e);
                //Write(e.Message);
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
                var isHotkeyPressed = ConsoleKeyHelpers.IsModifierPressed(keyInfo);
                var isAllowedKey = allowedKeys.Contains(keyInfo.Key) || allowedKeys == KeyGroups.NoKeys;
                if (isHotkeyPressed)
                {
                    Hotkey.Execute(keyInfo, this);
                }
                else if (isAllowedKey)
                {
                    return keyInfo;
                }
                else
                {
                    ProgramInput.RemoveLastReadKey();
                    SubmitNews($"{keyInfo.KeyChar} is not a currently recognized input.");
                }
                return ReadKey(allowedKeys);
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
                if (key.Key == ConsoleKey.Backspace) BackspaceFromLine(line, echo);
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
    }
}