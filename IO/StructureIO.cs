﻿using Structure.Code;
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
        private readonly NewsPrinter _newsPrinter;
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

        public void Read(Action<string> continuation, params ConsoleKey[] submitKeys) => Read(continuation, KeyGroups.NoKeys, submitKeys, echo: true);

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
            }, ConsoleKey.Enter);
        }

        public void ReadKey(Action<string> continuation) => Read(continuation, KeyGroups.MiscKeys, KeyGroups.NoKeys, echo: false);
        
        public void PromptOptions(string prompt, bool useDefault, params UserAction[] options)
        {
            var keyedOptions = CreateOptionKeysDictionary(options);
            Write($"{prompt}\n");
            keyedOptions.All(x => Write($"{x.Key}: {x.Value.Description}"));

            ConsoleKeyInfo key;

            key = ReadKey(KeyGroups.NoKeys);
            if (useDefault && !keyedOptions.ContainsKey(key.Key))
            {
                options.Last().Action();
            }
            else if (keyedOptions.ContainsKey(key.Key))
            {
                keyedOptions[key.Key].Action();
            }
        }

        public static (ConsoleKey Key, UserAction Action)[] CreateOptionKeys(UserAction[] options)
        {
            if (options == null) return null;
            var keys = new List<(ConsoleKey Key, UserAction Action)>();
            foreach (var option in options)
            {
                var possibleKeys = $"{option.Description.ToLower(CultureInfo.CurrentCulture)}abcdefghijklmnopqrstuvwxyz1234567890";
                for (int i = 0; i < possibleKeys.Length; i++)
                {
                    if (!keys.Any(x => x.Key == ConvertCharToConsoleKey(possibleKeys[i]).Key))
                    {
                        keys.Add((ConvertCharToConsoleKey(possibleKeys[i]).Key, option));
                        break;
                    }
                }
            }
            return keys.ToArray();
        }

        public static ConsoleKeyInfo ConvertCharToConsoleKey(char character)
        {
            if (Enum.TryParse(character.ToString(CultureInfo.InvariantCulture), true, out ConsoleKey consoleKey))
            {
                return new ConsoleKeyInfo(character, consoleKey, false, false, false);
            }
            else if (character == ' ')
                return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
            throw new InvalidOperationException($"Unable to convert '{character}' to ConsoleKey");
        }

        public static Dictionary<ConsoleKey, UserAction> CreateOptionKeysDictionary(UserAction[] options)
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

        public ConsoleKeyInfo ReadKey(ConsoleKey[] allowedKeys)
        {
            while (true)
            {
                while (!ProgramInput.IsKeyAvailable())
                {
                    if (!_newsPrinter.PrintNews(ProgramOutput)) break;
                    Thread.Sleep(10);
                }

                // TODO: Pass allowed keys in here all the time.
                var key = ProgramInput.ReadKey();
                var keyInfo = key.GetKeyInfo();
                CurrentTime.SetArtificialTime(key.Time);
                var wasHotkeyPressed = IsModifierPressed(keyInfo);
                if (wasHotkeyPressed)
                {
                    Hotkey.Execute(key.GetKeyInfo(), this);
                }
                else //if (allowedKeys.Contains(keyInfo.Key))
                {
                    return keyInfo;
                }
                if (wasHotkeyPressed) continue;
            }
        }

        public void Read(
            Action<string> continuation,
            ConsoleKey[] allowedKeys,
            ConsoleKey[] submitKeys,
            bool echo)
        {
            var line = new StringBuilder();
            while(true)
            {
                while (!ProgramInput.IsKeyAvailable())
                {
                    if (!_newsPrinter.PrintNews(ProgramOutput)) break;
                    Thread.Sleep(10);
                }

                var key = ReadKey(allowedKeys);
                ProcessReadKeyIntoLine(key, line, echo, allowedKeys);
                
                if (submitKeys.Contains(key.Key)) break;
                if (submitKeys == KeyGroups.NoKeys) break;
            }
            if (echo) Write();
            continuation?.Invoke(line.ToString());
        }

        private void ProcessReadKeyIntoLine(ConsoleKeyInfo key, StringBuilder line, bool echo, ConsoleKey[] allowedKeys)
        {
            if (IsAlphanumeric(key) || key.Key == ConsoleKey.OemPeriod || key.Key == ConsoleKey.Decimal)
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