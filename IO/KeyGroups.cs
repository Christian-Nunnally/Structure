using System;
using System.Linq;

namespace Structure.IO
{
    public static class KeyGroups
    {
        public static readonly ConsoleKey[] MiscKeys = new ConsoleKey[]
        {
            ConsoleKey.Enter,
            ConsoleKey.UpArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.Delete,
            ConsoleKey.Escape
        };
        public static readonly ConsoleKey[] NoKeys = Array.Empty<ConsoleKey>();

        public static readonly ConsoleKey[] SpaceKey = new ConsoleKey[]
        {
            ConsoleKey.Spacebar,
        };

        public static readonly ConsoleKey[] NumberKeys = new ConsoleKey[]
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

        public static readonly ConsoleKey[] LetterKeys = new ConsoleKey[]
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

        public static readonly ConsoleKey[] AlphanumericKeys = NumberKeys.Concat(LetterKeys).ToArray();

        public static readonly ConsoleKey[] AlphanumericKeysPlus = AlphanumericKeys.Concat(SpaceKey).ToArray();
    }
}
