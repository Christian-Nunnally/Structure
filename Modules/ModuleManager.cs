using Structure.IO;
using Structure.Modules.Interface;
using Structure.Structure;
using System;
using System.Collections.Generic;

namespace Structure.Modules
{
    public class ModuleManager : StructureModule
    {
        public const string ManageModulesPrompt = "Select action";
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
            IO.Write("MODULES\n\n\nSTATE\t\tNAME");
            _managedModules.All(m => IO.Write(ModuleString(m)));
            IO.Write("\n");

            var options = new List<UserAction>();
            foreach (var module in _managedModules)
            {
                var upgradeOption = new UserAction($"Upgrade {module.Name}", () => UpgradeModule(module));
                if (module is IObsoleteModule) options.Add(upgradeOption);
                var enableDisableOption = module.Enabled
                    ? new UserAction($"Disable {module.Name}", () => DisableModule(module))
                    : new UserAction($"Enable {module.Name}", () => EnableModule(module));
                options.Add(enableDisableOption);
            }
            options.Add(new UserAction("", () => { }, ConsoleKey.Escape));
            options.Add(new UserAction("Exit", () => { }, ConsoleKey.Enter));
            options.Add(new UserAction("", () => { }, ConsoleKey.LeftArrow));
            IO.ReadOptions(ManageModulesPrompt, "", options.ToArray());
        }

        private static string ModuleString(IModule module)
        {
            var upgradeable = module is IObsoleteModule;
            var state = module.Enabled ? "enabled" : "disabled";
            if (upgradeable) state += "+";
            return $"[{state}]\t{module.Name}";
        }

        private void EnableModule(IModule module)
        {
            if (_managedModules.Contains(module))
            {
                module.Enable(IoC, IO);
                IoC.Get<INewsPrinter>().EnqueueNews($"+{module.Name} enabled.");
            }
        }

        private void DisableModule(IModule module)
        {
            if (_managedModules.Contains(module))
            {
                module.Disable();
                IoC.Get<INewsPrinter>().EnqueueNews($"+{module.Name} disabled.");
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
                    IoC.Get<INewsPrinter>().EnqueueNews($"Upgraded {module.Name} to {upgradedModule.Name}");
                    if (wasEnabled) EnableModule(upgradedModule);
                }
            }
        }
    }
}