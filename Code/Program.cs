using System;
using System.Collections.Generic;
using static Structure.IO;

namespace Structure
{
    public class Program
    {
        public static bool Exit = false;
        public static List<Action> RegularActions = new List<Action>();

        private static void Start()
        {
            Write("Structure v1.0.1");
            Hotkey.Print();
            ReadAny();
        }

        private static void Main(string[] _)
        {
            Modules.SystemModules.All(x => x.Enable());
            while (!Exit) Run(Start);
        }
    }
}