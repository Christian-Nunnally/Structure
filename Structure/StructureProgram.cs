using Structure.IO;
using Structure.Modules.Interface;
using Structure.Modules.Obsolete;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Structure.Structure
{
    public class StructureProgram
    {
        private readonly StructureIO _io;
        private readonly Hotkey _hotkey;
        private readonly StructureData _data;
        private readonly IModule[] _modules;

        public bool Exit { get; set; }

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public StructureProgram(StructureIO io, StructureData data, IModule[] modules)
        {
            Contract.Requires(io != null);
            _io = io;
            _hotkey = io.Hotkey;
            _modules = modules;
            _data = data;
        }

        public void Run()
        {
            var manager = _modules.OfType<ModuleManager>().First();
            manager.RegisterModules(_modules);
            manager.Enable(_io, _hotkey, _data);
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
