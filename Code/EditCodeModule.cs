using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Structure
{
    internal class EditCodeModule : Module
    {
        public override int RequiredLevel => 3;

        public override IEnumerable<(char, string, Action)> GetOptions()
        {
            return new List<(char, string, Action)> { ('r', "Edit code", EditCode) };
        }

        private void EditCode()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe";
            startInfo.Arguments = Data.SolutionFilePath;
            var process = Process.Start(startInfo);
            IO.Write("Waiting for devenv to exit...");
            process.WaitForExit();
            IO.PromptYesNo("Would you like to rebuild and restart Structure?", RebuildAndRestart);
        }

        private void RebuildAndRestart()
        {
            Program.ShouldExit = true;
        }
    }
}