using System;
using System.Linq;
using System.Reflection;

namespace Structure
{
    public static class Program
    {
        public static bool Exit = false;

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public static string TitleString => $"Structure {Version.Major}.{Version.Minor}.{Version.Build}";

        public static void Main()
        {
            var manager = StartingModules.UserModules.OfType<ModuleManager>().First();
            manager.RegisterModules(StartingModules.UserModules);
            manager.Enable();
            while (!Exit) IO.Run(Start);
        }

        private static void Start()
        {
            IO.Write(TitleString);
            Hotkey.Print();
            IO.ReadAny();
        }
    }
}