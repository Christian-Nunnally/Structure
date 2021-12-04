using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Structure.IO;

namespace Structure
{
    public static class Program
    {
        public static bool Exit = false;
        public static List<Action> RegularActions = new List<Action>();
        private static Version _cachedVersion;
        public static Version Version => _cachedVersion ?? (_cachedVersion = Assembly.GetExecutingAssembly().GetName().Version);
        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public static void Main()
        {
            var moduleManager = Modules.UserModules.OfType<ModuleManager>().First();
            moduleManager.RegisterModules(Modules.UserModules);
            moduleManager.Enable();
            while (!Exit) Run(Start);
        }

        private static void Start()
        {
            Write(TitleString);
            Hotkey.Print();
            ReadAny();
        }
    }
}