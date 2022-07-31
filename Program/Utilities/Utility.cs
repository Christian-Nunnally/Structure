using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Structur.Program.Utilities
{
    public static class Utility
    {
        public static void All<T>(this IEnumerable<T> collection, Action<T> action)
        {
            Contract.Requires(collection != null);
            foreach (var item in collection) action(item);
        }

        public static string KeyToKeyString(ConsoleKeyInfo key)
        {
            return key.Key switch
            {
                ConsoleKey.UpArrow => "↑",
                ConsoleKey.DownArrow => "↓",
                ConsoleKey.LeftArrow => "←",
                ConsoleKey.RightArrow => "→",
                ConsoleKey.Enter => "<Enter>",
                ConsoleKey.Escape => "<Escape>",
                ConsoleKey.Backspace => "<Backspace>",
                ConsoleKey.Delete => "<Delete>",
                _ => $"{key.KeyChar}",
            };
        }

        public static char KeyToKeyChar(ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.A => 'a',
                ConsoleKey.B => 'b',
                ConsoleKey.C => 'c',
                ConsoleKey.D => 'd',
                ConsoleKey.E => 'e',
                ConsoleKey.F => 'f',
                ConsoleKey.G => 'g',
                ConsoleKey.H => 'h',
                ConsoleKey.I => 'i',
                ConsoleKey.J => 'j',
                ConsoleKey.K => 'k',
                ConsoleKey.L => 'l',
                ConsoleKey.M => 'm',
                ConsoleKey.N => 'n',
                ConsoleKey.O => 'o',
                ConsoleKey.P => 'p',
                ConsoleKey.Q => 'q',
                ConsoleKey.R => 'r',
                ConsoleKey.S => 's',
                ConsoleKey.T => 't',
                ConsoleKey.U => 'u',
                ConsoleKey.V => 'v',
                ConsoleKey.W => 'w',
                ConsoleKey.X => 'x',
                ConsoleKey.Y => 'y',
                ConsoleKey.Z => 'z',
                _ => ' ',
            };
        }
    }
}