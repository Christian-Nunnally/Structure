using System;
using System.Linq;
using static Structure.Data;
using static Structure.IO;
using static Structure.Modules;

namespace Structure
{
    internal class ModuleManager : Module
    {
        private UserAction _action;

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.L, _action);
        }

        public override void Enable()
        {
            _action = Hotkey.Add(ConsoleKey.L, new UserAction("Manage modules", ManageModules));
            EnabledModules.All(x => UserModules.FirstOrDefault(y => x == y.Name)?.Enable());
        }

        private void ManageModules()
        {
            Write("Enable/disable modules:");
            UserModules.All(m => Write(ModuleString(m)));
            Read(ToggleModule);
        }

        private string ModuleString(IModule x) => $"{UserModules.ToList().IndexOf(x)}: {x.Name} {(EnabledModules.Contains(x.Name) ? "(enabled)" : "(disabled)")}";

        private void ToggleModule(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return;
            if (Level < EnabledModules.Count) News($"{EnabledModules.Count}/{Level} modules enabled. Level up to enable more.");
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
                    if (CharacterBonus < 0)
                    {
                        News($"CharacterBonus ({CharacterBonus}) must be positive to enable a module.");
                        return;
                    }
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