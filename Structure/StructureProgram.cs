using Structure.Modules;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Structure
{
    public class StructureProgram
    {
        private readonly StructureIO _io;
        private readonly Hotkey _hotkey;

        public bool Exit { get; set; }

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public StructureProgram(StructureIO io)
        {
            Contract.Requires(io != null);
            _io = io;
            _hotkey = io.Hotkey;
        }

        public void Run()
        {
            var modules = StartingModules.CreateStartingModules();
            var manager = modules.OfType<ModuleManager>().First();
            var data = new StructureData();

            manager.RegisterModules(modules);
            manager.Enable(_io, _hotkey, data);
            new AnalyzeTaskCount().Enable(_io, _hotkey, data);
            while (!Exit) _io.Run(Loop);
        }

        private void Loop()
        {
            _io.Write(TitleString);
            _hotkey.Print(_io);
            _io.Read(x => { }, KeyGroups.NoKeys, KeyGroups.NoKeys);
        }
    }
}
