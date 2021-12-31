using System;
using System.Diagnostics;

namespace Structure
{
    internal class Diagnostics : StructureModule
    {
        private UserAction _action;

        public void DiagnosticOptions()
        {
            IO.PromptOptions("Diagnostics", false, new UserAction("Open save file", OpenSaveFile));
        }

        public void OpenSaveFile()
        {
            Process.Start(new ProcessStartInfo($"explorer", StructureFileIO.SavePath));
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.D, _action);
        }

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.D, new UserAction("Diagnostics", DiagnosticOptions));
        }
    }
}