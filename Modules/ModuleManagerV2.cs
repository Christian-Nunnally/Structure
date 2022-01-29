using Structure.Code.Modules;
using System;
using System.Collections.Generic;

namespace Structure
{
    public class ModuleManagerV2 : StructureModule
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
            var options = new List<UserAction>();
            foreach (var module in _listedModules)
            {
                var moduleString = ModuleString(module);
                if (module is IObsoleteModule)
                {
                    options.Add(new UserAction($"Upgrade {moduleString}", () => UpgradeModule(module)));
                }
                if (module.Enabled)
                {
                    options.Add(new UserAction($"Disable {moduleString}", () => DisableModule(module)));
                }
                else
                {
                    options.Add(new UserAction($"Enable {moduleString}", () => EnableModule(module)));
                }
            }

            IO.PromptOptions(ManageModulesPrompt, false, options.ToArray());
        }

        private static string ModuleString(IModule module)
        {
            var state = module.Enabled ? "enabled" : "disabled";
            state += module is IObsoleteModule ? "/upgradable" : "";
            return $"{module.Name} ({state})";
        }

        private void EnableModule(IModule module)
        {
            if (_listedModules.Contains(module))
            {
                module.Enable(IO, Hotkey, Data);
                IO.News($"+{module.Name} enabled.");
            }
        }

        private void DisableModule(IModule module)
        {
            if (_listedModules.Contains(module))
            {
                module.Disable();
                IO.News($"+{module.Name} disabled.");
            }
        }

        private void UpgradeModule(IModule module)
        {
            var index = _listedModules.IndexOf(module);
            if (index >= 0 && index < _listedModules.Count)
            {
                if (module is IObsoleteModule obsoleteModule)
                {
                    var upgradedModule = obsoleteModule.UpgradeModule();
                    _listedModules[index] = upgradedModule;
                    IO.News($"Upgraded {module.Name} to {upgradedModule.Name}");
                }
            }
        }
    }
}