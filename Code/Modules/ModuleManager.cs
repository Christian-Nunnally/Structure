using System;
using System.Linq;
using static Structure.Data;
using static Structure.IO;
using static Structure.Modules;

namespace Structure
{
    public class ModuleManager : Module
    {
        public const string ManageModulesPrompt = "Enable/disable modules:";
        private UserAction _action;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.L, _action);
        }

        public override void Enable()
        {
            _action = new UserAction("Manage modules", ManageModules);
            Hotkey.Add(ConsoleKey.L, _action);
            EnabledModules.All(x => UserModules.FirstOrDefault(y => x == y.Name)?.Enable());
        }

        private void ManageModules()
        {
            Write(ManageModulesPrompt);
            // TODO: Make this show pages at a time.
            UserModules.All(m => Write(ModuleString(m)));
            Read(ToggleModule);
        }

        private string ModuleString(IModule x) => $"{UserModules.ToList().IndexOf(x)}: {x.Name} {(EnabledModules.Contains(x.Name) ? "(enabled)" : "(disabled)")}";

        private void ToggleModule(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return;
            else if (int.TryParse(module, out var index) && index >= 0 && index < UserModules.Length) ToggleModule(index);
        }

        private void ToggleModule(int index)
        {
            if (index >= 0 && index < UserModules.Length)
            {
                var module = UserModules[index];
                var name = module.Name;
                if (!EnabledModules.Contains(name))
                {
                    EnabledModules.Add(name);
                    module.Enable();
                    News($"+{name} enabled.");
                }
                else
                {
                    EnabledModules.Remove(name);
                    module.Disable();
                    News($"+{name} disabled.");
                }
            }
        }
    }
}