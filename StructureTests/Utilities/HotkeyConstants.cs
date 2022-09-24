using System;

namespace StructureTests.Utilities
{
    public static class HotkeyConstants
    {
        public static readonly ConsoleKeyInfo StartTaskTreeModuleHotkey = new('t', ConsoleKey.T, false, false, true);
        public static readonly ConsoleKeyInfo ExitModuleHotkey = new('\u241B', ConsoleKey.Escape, false, false, false);
        public static readonly ConsoleKeyInfo AHotkey = new('a', ConsoleKey.A, false, false, false);
        public static readonly ConsoleKeyInfo EscapeHotkey = new('\u241B', ConsoleKey.Escape, false, false, false);
        public static readonly ConsoleKeyInfo SubmitHotkey = new('\u2386', ConsoleKey.Enter, false, false, false);
    }
}