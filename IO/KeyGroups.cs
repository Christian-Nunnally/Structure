using System;
using System.Linq;

namespace Structure
{
    public static class KeyGroups
    {
        public static ConsoleKey[] MiscKeys = new ConsoleKey[]
        {
            ConsoleKey.Enter,
            ConsoleKey.UpArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.Delete,
            ConsoleKey.Escape
        };
        public static ConsoleKey[] NoKeys = new ConsoleKey[0];

        public static ConsoleKey[] NumberKeys = new ConsoleKey[]
        {
            ConsoleKey.NumPad0,
            ConsoleKey.NumPad1,
            ConsoleKey.NumPad2,
            ConsoleKey.NumPad3,
            ConsoleKey.NumPad4,
            ConsoleKey.NumPad5,
            ConsoleKey.NumPad6,
            ConsoleKey.NumPad7,
            ConsoleKey.NumPad8,
            ConsoleKey.NumPad9,
            ConsoleKey.D0,
            ConsoleKey.D1,
            ConsoleKey.D2,
            ConsoleKey.D3,
            ConsoleKey.D4,
            ConsoleKey.D5,
            ConsoleKey.D6,
            ConsoleKey.D7,
            ConsoleKey.D8,
            ConsoleKey.D9,
            ConsoleKey.OemPeriod,
            ConsoleKey.Decimal,
        };

        public static ConsoleKey[] LetterKeys = new ConsoleKey[]
        {
            ConsoleKey.A,
            ConsoleKey.B,
            ConsoleKey.C,
            ConsoleKey.D,
            ConsoleKey.E,
            ConsoleKey.F,
            ConsoleKey.G,
            ConsoleKey.H,
            ConsoleKey.I,
            ConsoleKey.J,
            ConsoleKey.K,
            ConsoleKey.L,
            ConsoleKey.M,
            ConsoleKey.N,
            ConsoleKey.O,
            ConsoleKey.P,
            ConsoleKey.Q,
            ConsoleKey.R,
            ConsoleKey.S,
            ConsoleKey.T,
            ConsoleKey.U,
            ConsoleKey.V,
            ConsoleKey.W,
            ConsoleKey.X,
            ConsoleKey.Y,
            ConsoleKey.Z,
        };

        public static ConsoleKey[] AlphanumericKeys = NumberKeys.Concat(LetterKeys).ToArray();
    }
}
