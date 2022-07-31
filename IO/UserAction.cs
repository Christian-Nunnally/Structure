using Structur.Program.Utilities;
using System;

namespace Structur.IO
{
    public class UserAction
    {
        public string Description { get; set; }

        public Action Action { get; set; }

        public ConsoleKeyInfo Hotkey { get; set; }

        public bool HotkeyOverridden { get; set; }

        public UserAction(string description, Action action)
        {
            Description = description;
            Action = action;
        }

        public UserAction(string description, Action action, ConsoleKey hotkey) : this(description, action)
        {
            Hotkey = new ConsoleKeyInfo(Utility.KeyToKeyChar(hotkey), hotkey, false, false, false);
            HotkeyOverridden = true;
        }
    }
}