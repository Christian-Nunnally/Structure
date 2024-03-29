﻿using System;

namespace Structur.IO.Output
{
    public class ConsoleOutput : IProgramOutput
    {
        public ConsoleOutput() => Console.OutputEncoding = System.Text.Encoding.Unicode;

        public int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }

        public int CursorTop { get => Console.CursorTop; set => Console.CursorTop = value; }

        public bool CursorVisible
        {
            get => !OperatingSystem.IsWindows() || Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        public int Width => Console.WindowWidth;

        public int Height => Console.WindowHeight;

        public void Clear() => Console.Clear();

        public void Write(string text) => Console.Write(text);

        public void WriteLine(string text) => Console.WriteLine(text);
    }
}