using Structure.IO;
using Structure.Modules.Interface;
using Structure.Modules.Obsolete;
using System;
using System.Collections.Generic;

namespace Structure.Modules
{
    public class ModuleManagerV2 : StructureModule
    {
        public const string ManageModulesPrompt = "Manage modules:";
        private readonly List<IModule> _managedModules = new List<IModule>();
        private UserAction _action;

        internal void RegisterModules(IModule[] modules) => _managedModules.AddRange(modules);

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
            foreach (var module in _managedModules)
            {
                var moduleString = ModuleString(module);
                var upgradeOption = new UserAction($"Upgrade {moduleString}", () => UpgradeModule(module));
                if (module is IObsoleteModule) options.Add(upgradeOption);
                var enableDisableOption = module.Enabled
                    ? new UserAction($"Disable {moduleString}", () => DisableModule(module))
                    : new UserAction($"Enable {moduleString}", () => EnableModule(module));
                options.Add(enableDisableOption);
            }

            IO.PromptOptions(ManageModulesPrompt, false, "", options.ToArray());
        }

        private static string ModuleString(IModule module)
        {
            var state = module.Enabled ? "enabled" : "disabled";
            state += module is IObsoleteModule ? "/upgradable" : "";
            return $"{module.Name} ({state})";
        }

        private void EnableModule(IModule module)
        {
            if (_managedModules.Contains(module))
            {
                module.Enable(IO, Hotkey, Data);
                IO.SubmitNews($"+{module.Name} enabled.");
            }
        }

        private void DisableModule(IModule module)
        {
            if (_managedModules.Contains(module))
            {
                module.Disable();
                IO.SubmitNews($"+{module.Name} disabled.");
            }
        }

        private void UpgradeModule(IModule module)
        {
            var index = _managedModules.IndexOf(module);
            if (index > 0)
            {
                if (module is IObsoleteModule obsoleteModule)
                {
                    var wasEnabled = module.Enabled;
                    if (wasEnabled) DisableModule(module);
                    var upgradedModule = obsoleteModule.UpgradeModule();
                    _managedModules[index] = upgradedModule;
                    IO.SubmitNews($"Upgraded {module.Name} to {upgradedModule.Name}");
                    if (wasEnabled) EnableModule(upgradedModule);
                }
            }
        }
    }
}