using System;

namespace StructureTests.Utilities
{
    public static class HotkeyConstants
    {
        public static readonly ConsoleKeyInfo StartTaskTreeModuleHotkey = new('t', ConsoleKey.T, false, false, true);
        public static readonly ConsoleKeyInfo ExitModuleHotkey = new('\u241B', ConsoleKey.Escape, false, false, false);
        public static readonly ConsoleKeyInfo AHotkey = new('a', ConsoleKey.A, false, false, false);
        public static readonly ConsoleKeyInfo BHotkey = new('b', ConsoleKey.B, false, false, false);
        public static readonly ConsoleKeyInfo EscapeHotkey = new('\u241B', ConsoleKey.Escape, false, false, false);
        public static readonly ConsoleKeyInfo SubmitHotkey = new('\u2386', ConsoleKey.Enter, false, false, false);
        public static readonly ConsoleKeyInfo BackspaceHotkey = new('\u0008', ConsoleKey.Backspace, false, false, false);

        public static readonly ConsoleKeyInfo UpArrowHotkey = new('\u2191', ConsoleKey.UpArrow, shift: false, alt: false, control: false);
        public static readonly ConsoleKeyInfo DownArrowHotkey = new('\u2193', ConsoleKey.DownArrow, shift: false, alt: false, control: false);
        public static readonly ConsoleKeyInfo LeftArrowHotkey = new('\u2190', ConsoleKey.LeftArrow, shift: false, alt: false, control: false);
        public static readonly ConsoleKeyInfo RightArrowHotkey = new('\u2192', ConsoleKey.RightArrow, shift: false, alt: false, control: false);
    }
}