using System;

namespace StructureTests.Utilities
{
    public static class HotkeyConstants
    {
        public static readonly ConsoleKeyInfo StartTaskTreeModuleHotkey = new('t', ConsoleKey.T, false, false, true);
        public static readonly ConsoleKeyInfo ExitModuleHotkey = new('\u001B', ConsoleKey.Escape, false, false, false);
    }
}