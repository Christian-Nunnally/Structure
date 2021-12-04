using Structure.Code.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using static Structure.IO;

namespace Structure
{
    public class ModuleManager : Module
    {
        public const string ManageModulesPrompt = "Enable/disable modules:";
        private readonly List<IModule> _listedModules = new List<IModule>();
        private UserAction _action;

        internal void RegisterModules(IModule[] modules)
        {
            _listedModules.AddRange(modules);
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.L, _action);
        }

        protected override void OnEnable()
        {
            _action = new UserAction("Manage modules", ManageModules);
            Hotkey.Add(ConsoleKey.L, _action);
        }

        private void ManageModules()
        {
            Write(ManageModulesPrompt);
            Write("Type '1' to enable or disable module 1");
            Write("Type 'upgrade 1' to upgrade module 1");
            // TODO: Make this show pages at a time.
            _listedModules.All(m => Write(ModuleString(m)));
            Read(ToggleModule);
        }

        private string ModuleString(IModule module)
        {
            var enabledDisabledString = module is IObsoleteModule
                ? "(upgradable)"
                : module.Enabled ? "(enabled)" : "(disabled)";
            return $"{_listedModules.ToList().IndexOf(module)}: {module.Name} {enabledDisabledString}";
        }

        private void ToggleModule(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return;
            bool upgrade = module.Contains("upgrade ");
            var length = "upgrade ".Length;
            module = upgrade ? module[length..] : module;
            if (int.TryParse(module, out var index) && index >= 0 && index < _listedModules.Count) ToggleModule(index, upgrade);
        }

        private void ToggleModule(int index, bool upgrade)
        {
            if (index >= 0 && index < _listedModules.Count)
            {
                var module = _listedModules[index];
                var name = module.Name;
                if (!module.Enabled)
                {
                    if (module is IObsoleteModule obsoleteModule && upgrade)
                    {
                        var upgradedModule = obsoleteModule.UpgradedModule;
                        _listedModules[index] = upgradedModule;
                        News($"Upgraded {name} to {upgradedModule.Name}");
                        module = upgradedModule;
                    }

                    module.Enable();
                    News($"+{name} enabled.");
                }
                else
                {
                    module.Disable();
                    News($"+{name} disabled.");

                    if (module is IObsoleteModule obsoleteModule && upgrade)
                    {
                        _listedModules[index] = obsoleteModule.UpgradedModule;
                        News($"Upgraded {name} to {obsoleteModule.UpgradedModule.Name}");
                    }
                }
            }
        }
    }
}