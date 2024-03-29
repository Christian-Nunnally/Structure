﻿using Structur.IO;
using Structur.Modules.Interfaces;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Structur.Program
{
    public class StructureProgram
    {
        private readonly StructureIO _io;
        private readonly StructureIoC _ioc;
        private readonly Hotkey _hotkey;
        private readonly IModule[] _modules;

        public bool Exit { get; set; }

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure v{Version.Major}.{Version.Minor}.{Version.Build}";

        public StructureProgram(StructureIoC ioc, StructureIO io, IModule[] modules)
        {
            Contract.Requires(io != null);
            Console.Title = TitleString;
            _ioc = ioc;
            _io = io;
            _hotkey = ioc?.Get<Hotkey>();
            _modules = modules;
        }

        public void Run(ExitToken exitToken)
        {
            var manager = _modules.OfType<IModuleManager>().First();
            manager.RegisterModules(_modules);
            manager.Enable(_ioc, _io);
            while (!exitToken.Exit) Loop();
        }

        private void Loop()
        {
            _io.ClearBuffer();
            _io.Write(TitleString);
            _hotkey.Print(_io);
            _io.ClearStaleOutput();
            _io.Read(x => { }, KeyGroups.NoKeys, KeyGroups.NoKeys);
        }
    }
}
