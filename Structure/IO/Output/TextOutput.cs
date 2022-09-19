using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Structur.IO.Output
{
    public class TextOutput : IProgramOutput
    {
        private int _cursorLeft;
        private int _cursorTop;
        private bool _cursorVisible;
        private bool _disabled;
        private readonly StringBuilder _screen = new();
        private List<string> _debuggingStrings = new();

        public IList<string> Screens { get; } = new List<string>();

        public int CursorLeft { get => _cursorLeft; set => _cursorLeft = value; }

        public int CursorTop { get => _cursorTop; set => _cursorTop = value; }

        public bool CursorVisible { get => _cursorVisible; set => _cursorVisible = value; }

        public int Width => 100;

        public int Height => 100;

        public void Clear() 
        {
            Screens.Add(_screen.ToString());
            _screen.Clear();
        }

        public void Write(string text)
        {
            if (_disabled) return;
            var currentText = _screen.ToString().Replace(Environment.NewLine, "\n", StringComparison.Ordinal);
            _debuggingStrings.Add(currentText);
            var splitText = currentText.Split('\n').ToList();
            while (splitText.Count <= CursorTop) splitText.Add(string.Empty);
            while (splitText[CursorTop].Length < CursorLeft) splitText[CursorTop] = string.Concat(splitText[CursorTop], " ");
            var textLength = text.Length;
            var currentLine = splitText[CursorTop];
            var prefix = currentLine[..CursorLeft];
            var postfix = currentLine.Length > CursorLeft + textLength ? currentLine[(CursorLeft + textLength)..] : string.Empty;
            splitText[CursorTop] = prefix + text + postfix;
            _screen.Clear();
            foreach (var line in splitText) _screen.Append(line + '\n');
            _screen.Remove(_screen.Length - 1, 1);
            CursorTop += text.Count(c => c == '\n');
            CursorLeft += text.Contains('\n', StringComparison.OrdinalIgnoreCase) ? text.Length - text.LastIndexOf('\n') - 1 : text.Length;
        }

        public void WriteDebugStrings()
        {
            Console.WriteLine("TextOutput Debugging strings >>>>>");
            foreach (var debuggingString in _debuggingStrings)
            {
                Console.WriteLine("------------");
                Console.WriteLine(debuggingString);
                Console.WriteLine("------------");
            }
            Console.WriteLine("TextOutput Debugging strings <<<<<");
        }

        public void Disable()
        {
            _disabled = true;
        }

        public void WriteLine(string text)
        {
            Write(text);
            CursorTop += 1;
            CursorLeft = 0;
        }

        public string Read() => _screen.ToString();
    }
}