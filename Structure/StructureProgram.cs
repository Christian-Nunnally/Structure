using Structure.Code;
using System;
using System.Linq;
using System.Reflection;

namespace Structure
{
    public class StructureProgram
    {
        public bool Exit = false;

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public void Run()
        {
            var modules = StartingModules.CreateStartingModules();
            var manager = modules.OfType<ModuleManager>().First();
            var data = new CommonData();
            var hotkey = new Hotkey();
            var io = new StructureIO(hotkey);
            var input = new StructureInput();
            input.InitializeStructureInput(io);
            var output = new ConsoleOutput();
            io.SetInput(input);
            io.SetOutput(output);
            manager.RegisterModules(modules);
            manager.Enable(io, hotkey, data);
            while (!Exit) io.Run(() => Loop(io, hotkey));
        }

        private void Loop(StructureIO io, Hotkey hotkey)
        {
            io.Write(TitleString);
            hotkey.Print(io);
            io.ReadAny();
        }
    }
}
