using System;
using System.Diagnostics;

namespace Structure
{
    internal class Diagnostics : Module
    {
        private UserAction _action;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.D, _action);
        }

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.D, new UserAction("Diagnostics", DiagnosticOptions));
        }

        public void DiagnosticOptions()
        {
            IO.PromptOptions("Diagnostics", false, new UserAction("Open save file", OpenSaveFile));
        }

        public void OpenSaveFile()
        {
            Process.Start(new ProcessStartInfo($"explorer", FileIO.SavePath));
        }
    }
}