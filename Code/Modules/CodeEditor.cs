using System;
using System.Diagnostics;

namespace Structure
{
    internal class CodeEditor : Module
    {
        private UserAction _action;

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.Q, new UserAction("Edit code", () => IO.Run(EditCode)));
        }

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.Q, _action);
        }

        private void EditCode()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe",
                Arguments = FileIO.SolutionFilePath
            };
            var process = Process.Start(startInfo);
            IO.Write("Waiting for devenv to exit...");
            process.WaitForExit();
            IO.PromptYesNo("Exit structure to rebuild?", () => Program.Exit = true);
        }
    }
}