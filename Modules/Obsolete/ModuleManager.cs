using Structure.Code.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure
{
    public class ModuleManager : StructureModule, IObsoleteModule
    {
        public const string ManageModulesPromptText = "Enable/disable modules:";
        private const string UpgradeModuleHintText = "Type 'upgrade 1' to upgrade module 1";
        private const string ToggleModuleHintText = "Type '1' to enable or disable module 1";
        private readonly List<IModule> _listedModules = new List<IModule>();
        private UserAction _action;

        public IModule UpgradedModule => new ModuleManagerV2();

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
            IO.Write(ManageModulesPromptText);
            IO.Write(ToggleModuleHintText);
            IO.Write(UpgradeModuleHintText);
            // TODO: Make this show pages at a time.
            _listedModules.All(m => IO.Write(ModuleString(m)));
            IO.Read(ToggleModule, ConsoleKey.Enter);
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
            bool upgrade = module.Contains("upgrade ", StringComparison.OrdinalIgnoreCase);
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
                        IO.News($"Upgraded {name} to {upgradedModule.Name}");
                        module = upgradedModule;
                    }

                    module.Enable(IO, Hotkey, Data);
                    IO.News($"+{name} enabled.");
                }
                else
                {
                    module.Disable();
                    IO.News($"+{name} disabled.");

                    if (module is IObsoleteModule obsoleteModule && upgrade)
                    {
                        _listedModules[index] = obsoleteModule.UpgradedModule;
                        IO.News($"Upgraded {name} to {obsoleteModule.UpgradedModule.Name}");
                    }
                }
            }
        }
    }
}