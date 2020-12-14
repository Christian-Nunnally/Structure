using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class Program
    {
        public static bool ShouldExit = false;
        public static List<Action> RegularActions = new List<Action>();

        public Program()
        {
            Initialize();
            while (!ShouldExit)
            {
                Clear();
                Write("Structure");
                WriteHotkeys();
                ReadAny();
            }
        }

        public static List<IModule> Modules { get; private set; } = new List<IModule>();

        private static void Main(string[] _) => new Program();

        private void Initialize()
        {
            Modules.Add(new TaskModule());
            Modules.Add(new InfoModule());
            Modules.Add(new CharacterBonusPerFileModule());
            Modules.Add(new PrestiegeModule());
            Modules.Add(new EditCodeModule());
            new ToxinModule().Enable();
            new LevelUpModule().Enable();
            new CharacterBonusModule().Enable();
            new ManageModulesModule().Enable();
            Hotkeys.Add(ConsoleKey.Q, (PromptModuleOptions, "Actions"));
        }

        private void PromptModuleOptions()
        {
            var options = Modules
                .Where(m => EnabledModules.Contains(m.Name))
                .SelectMany(x => x.GetOptions())
                .ToArray();
            PromptOptions("What would you like to do?", false, options);
        }
    }
}