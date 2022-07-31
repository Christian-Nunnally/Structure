using Structur.IO;
using Structur.Modules.Interfaces;
using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structur.Modules
{
    public class ModuleManager : StructureModule, IModuleManager
    {
        private const string ModuleStartHoykeyPrompt = "Manage modules";
        public const string ManageModulesPrompt = "Select action";
        public const string UpgradeModulePrompt = "Pick module to upgrade";
        public const string EnableModulePrompt = "Pick module to enable";
        public const string DisableModulePrompt = "Pick module to disable";
        private readonly List<IModule> _managedModules = new List<IModule>();
        private UserAction _action;

        public void RegisterModules(IModule[] modules) => _managedModules.AddRange(modules);

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.L, _action);
        }

        protected override void OnEnable()
        {
            _action = new UserAction(ModuleStartHoykeyPrompt, ManageModules);
            Hotkey.Add(ConsoleKey.L, _action);
        }

        private void ManageModules()
        {
            PrintModuleStatusList();
            var options = GenerateModuleManagementOptions();
            IO.ReadOptions(ManageModulesPrompt, string.Empty, options);
        }

        private UserAction[] GenerateModuleManagementOptions()
        {
            var enableOptions = CreateExitOptionsList();
            var disableOptions = CreateExitOptionsList();
            var upgradeOptions = CreateExitOptionsList();
            foreach (var module in _managedModules)
            {
                var upgradeOption = new UserAction($"Upgrade {module.Name}", () => UpgradeModule(module));
                var disableOption = new UserAction($"Disable {module.Name}", () => DisableModule(module));
                var enableOption = new UserAction($"Enable {module.Name}", () => EnableModule(module));
                if (module is IObsoleteModule) upgradeOptions.Add(upgradeOption);
                if (!module.Enabled) enableOptions.Add(enableOption);
                else if (module is IModuleManager) continue;
                else disableOptions.Add(disableOption);
            }
            var options = CreateExitOptionsList();
            options.Add(new UserAction("Upgrade module", () => IO.Run(() => IO.ReadOptions(UpgradeModulePrompt, "", upgradeOptions.ToArray())), ConsoleKey.U));
            options.Add(new UserAction("Enable module", () => IO.Run(() => IO.ReadOptions(EnableModulePrompt, "", enableOptions.ToArray())), ConsoleKey.E));
            options.Add(new UserAction("Disable module", () => IO.Run(() => IO.ReadOptions(DisableModulePrompt, "", disableOptions.ToArray())), ConsoleKey.D));
            return options.ToArray();
        }

        private void PrintModuleStatusList()
        {
            IO.Write("MODULES\n\n\nSTATE      NAME");
            _managedModules.All(m => IO.Write(ModuleString(m)));
            IO.Write("\n");
        }

        private static List<UserAction> CreateExitOptionsList() => new List<UserAction>
        {
            new UserAction("Exit", () => { }, ConsoleKey.Enter),
            new UserAction("", () => { }, ConsoleKey.LeftArrow),
            new UserAction("", () => { }, ConsoleKey.Escape),
        };

        private static string ModuleString(IModule module)
        {
            var upgradeable = module is IObsoleteModule;
            var state = module.Enabled ? "enabled" : "disabled";
            if (upgradeable) state += "+";
            return $"[{state}]    {module.Name}";
        }

        private void EnableModule(IModule module)
        {
            if (!_managedModules.Contains(module)) return;
            module.Enable(IoC, IO);
            IoC.Get<INewsPrinter>().EnqueueNews($"+{module.Name} enabled.");
        }

        private void DisableModule(IModule module)
        {
            if (!_managedModules.Contains(module)) return;
            module.Disable();
            IoC.Get<INewsPrinter>().EnqueueNews($"+{module.Name} disabled.");
        }

        private void UpgradeModule(IModule module)
        {
            var index = _managedModules.IndexOf(module);
            if (index < 0) return;
            if (!(module is IObsoleteModule obsoleteModule)) return;
            var wasEnabled = module.Enabled;
            if (wasEnabled) DisableModule(module);
            var upgradedModule = obsoleteModule.UpgradeModule();
            _managedModules[index] = upgradedModule;
            IoC.Get<INewsPrinter>().EnqueueNews($"Upgraded {module.Name} to {upgradedModule.Name}");
            if (wasEnabled) EnableModule(upgradedModule);
        }
    }
}