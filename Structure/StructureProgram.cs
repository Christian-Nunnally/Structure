using Structure.IO;
using Structure.Modules.Interface;
using Structure.Server;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Structure.Structure
{
    public class StructureProgram
    {
        private readonly StructureIoC _ioc;
        private readonly StructureIO _io;
        private readonly Hotkey _hotkey;
        private readonly IModule[] _modules;

        public bool Exit { get; set; }

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public StructureProgram(StructureIoC ioc, StructureIO io, IModule[] modules)
        {
            Contract.Requires(io != null);
            _ioc = ioc;
            _io = io;
            _hotkey = ioc?.Get<Hotkey>();
            _modules = modules;
        }

        public void Run()
        {
            var manager = _modules.OfType<IModuleManager>().First();
            manager.RegisterModules(_modules);
            manager.Enable(_ioc, _io);
            while (!Exit) _io.Run(Loop);
        }

        private void Loop()
        {
            if (!_io.SkipUnescesscaryOperations)
            { 
                _io.Write(TitleString);
                _hotkey.Print(_io);
            }
            // TODO: Do not save keys from this input.
            _io.Read(x => { }, KeyGroups.NoKeys, KeyGroups.NoKeys);
        }
    }
}
