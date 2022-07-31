﻿namespace Structur.Server
{
    internal class InputResponse
    {
        public static InputResponse Create(string text)
        {
            return new InputResponse
            {
                ConsoleText = text,
            };
        }

        public string ConsoleText { get; set; }
    }
}