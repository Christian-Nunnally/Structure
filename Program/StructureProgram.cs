using Structure.IO;
using Structure.Modules.Interface;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Structure.Program
{
    public class StructureProgram
    {
        public readonly StructureIO IO;
        private readonly StructureIoC _ioc;
        private readonly Hotkey _hotkey;
        private readonly IModule[] _modules;

        public bool Exit { get; set; }

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public StructureProgram(StructureIoC ioc, StructureIO io, IModule[] modules)
        {
            Contract.Requires(io != null);
            _ioc = ioc;
            IO = io;
            _hotkey = ioc?.Get<Hotkey>();
            _modules = modules;
        }

        public void Run()
        {
            var manager = _modules.OfType<IModuleManager>().First();
            manager.RegisterModules(_modules);
            manager.Enable(_ioc, IO);
            while (!Exit) IO.Run(Loop);
        }

        private void Loop()
        {
            if (!IO.SkipUnescesscaryOperations)
            { 
                IO.Write(TitleString);
                _hotkey.Print(IO);
            }
            IO.Read(x => { }, KeyGroups.NoKeys, KeyGroups.NoKeys);
        }
    }
}
