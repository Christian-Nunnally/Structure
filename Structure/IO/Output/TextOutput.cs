using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Structur.IO.Output
{
    public class TextOutput : IProgramOutput
    {
        private readonly List<List<char>> _lines = new();

        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public bool CursorVisible { get; set; }

        public int Width => 100;

        public int Height => 100;

        public void Clear()
        {
            CursorLeft = 0;
            CursorTop = 0;
            _lines.Clear();
        }

        public void Write(string text)
        {
            foreach (var character in text) WriteCharacter(character);
        }

        private void WriteCharacter(char character)
        {
            while (_lines.Count <= CursorTop) _lines.Add(new List<char>());
            while (_lines[CursorTop].Count < CursorLeft) _lines[CursorTop].Add(' ');
            if (character == '\n')
            {
                CursorTop++;
                CursorLeft = 0;
            }
            else if (character == '\b')
            {
                if (CursorLeft == 0)
                {
                    if (CursorTop > 0)
                    {
                        CursorTop--;
                        CursorLeft = _lines[CursorTop].Count;
                    }
                }
                else CursorLeft--;
            }
            else
            {
                if (_lines[CursorTop].Count == CursorLeft) _lines[CursorTop].Add(character);
                else _lines[CursorTop][CursorLeft] = character;
                CursorLeft++;
            }
            if (CursorLeft >= Width)
            {
                CursorLeft = 0;
                CursorTop++;
            }
        }

        public void WriteLine(string text)
        {
            Write(text);
            CursorTop += 1;
            CursorLeft = 0;
        }

        public string Read()
        {
            var result = new StringBuilder();
            var lines = _lines.Select(l => new string(l.ToArray()));
            foreach (var line in lines) result.AppendLine(line);
            return $"{result}".TrimEnd();
        }
    }
}