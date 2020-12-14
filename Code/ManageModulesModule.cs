using System;
using System.Linq;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class ManageModulesModule : Module
    {
        public override int RequiredLevel => throw new NotImplementedException();

        public override void Disable()
        {
            Hotkeys.Remove(ConsoleKey.L);
        }

        public override void Enable()
        {
            Hotkeys.Add(ConsoleKey.L, (ManageModules, "Manage modules"));
            EnabledModules.All(EnableModule);
        }

        private void ManageModules() => PromptYesNo($"Enable a module?", () => Run(EnableModule, 3));

        private void EnableModule()
        {
            if (Level >= EnabledModules.Count)
            {
                Write("Pick which module to enable from the list below");
                Program.Modules.Where(m => m.RequiredLevel <= Level).All(WriteModuleString);
                ReadLine(EnableModule);
            }
            else Write($"You already have {EnabledModules.Count} modules enabled. Level up to enable more.");
        }

        private void WriteModuleString(IModule module) => Write(GetModuleString(module));

        private string GetModuleString(IModule x) => $"{Program.Modules.IndexOf(x)}: {x.Name} {(EnabledModules.Contains(x.Name) ? "(enabled)" : "(disabled)")} | Level {x.RequiredLevel}";

        private void EnableModule(string module)
        {
            var moduleIndex = Program.Modules.FindIndex(x => x.Name == module);
            if (int.TryParse(module, out var moduleIndexInt))
            {
                EnableModule(moduleIndexInt);
            }
            else if (moduleIndex >= 0)
            {
                EnableModule(moduleIndex, true);
            }
            else Write($"'{moduleIndex}' is not a valid module or module index.");
        }

        private void EnableModule(int index, bool alreadyInList = false)
        {
            if (index >= 0 && index < Program.Modules.Count)
            {
                var moduleName = Program.Modules[index].Name;
                if (!EnabledModules.Contains(moduleName) || alreadyInList)
                {
                    if (!alreadyInList) EnabledModules.Add(moduleName);
                    Program.Modules[index].Enable();
                    Write($"+{moduleName} enabled.");
                }
                else Write("Module already enabled.");
            }
            else Write("Module index out of range.");
        }
    }
}