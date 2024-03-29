﻿using Structur.IO;
using Structur.Modules.Interfaces;
using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structur.Modules.Obsolete
{
    public class ModuleManagerObsolete : StructureModule, IObsoleteModule, IModuleManager
    {
        public const string ManageModulesPromptText = "Enable/disable modules:";
        private const string UpgradeModuleHintText = "Type 'upgrade 1' to upgrade module 1";
        private const string ToggleModuleHintText = "Type '1' to enable or disable module 1";
        private readonly List<IModule> _listedModules = new List<IModule>();
        private UserAction _action;

        public IModule UpgradeModule()
        {
            var newModule = new ModuleManager();
            var thisIndex = _listedModules.IndexOf(this);
            var moduleListCopy = _listedModules.ToArray();
            if (thisIndex >= 0)
            {
                moduleListCopy[thisIndex] = newModule;
                newModule.RegisterModules(moduleListCopy);
                newModule.Enable(IoC, IO);
            }
            return newModule;
        }

        public void RegisterModules(IModule[] modules)
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
            _listedModules.All(m => IO.Write(ModuleString(m)));
            IO.Read(ToggleModule, KeyGroups.AlphanumericInputKeys, new[] { ConsoleKey.Enter });
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
            if (module == "asdf")
            {
                ToggleModule(_listedModules.IndexOf(_listedModules.OfType<ModuleManagerObsolete>().First()), true, _listedModules.OfType<ModuleManagerObsolete>().First());
                return;
            }
            if (string.IsNullOrWhiteSpace(module)) return;
            bool upgrade = module.Contains("upgrade ", StringComparison.OrdinalIgnoreCase);
            var length = "upgrade ".Length;
            module = upgrade ? module[length..] : module;
            if (int.TryParse(module, out var index) && index >= 0 && (index < _listedModules.Count || _indexMap.ContainsKey(index))) ToggleModule(index, upgrade);
        }

        private readonly Dictionary<int, Type> _indexMap = new Dictionary<int, Type> { {0,typeof(TreeTaskObsolete)},{ 3, typeof(RoutinerObsolete) },{ 7, typeof(ModuleManagerObsolete) } };

        private void ToggleModule(int index, bool upgrade)
        {
            IModule module = null;
            if (_indexMap.ContainsKey(index))
            {
                module = _listedModules.Where(x => x.GetType() == _indexMap[index]).FirstOrDefault();
            }

            if ((index >= 0 && index < _listedModules.Count) || module != null)
            {
                if (module == null) module = _listedModules[index];
                ToggleModule(index, upgrade, module);
            }
        }

        private void ToggleModule(int index, bool upgrade, IModule module)
        {
            var name = module.Name;
            if (!module.Enabled)
            {
                if (module is IObsoleteModule obsoleteModule && upgrade)
                {
                    var upgradedModule = obsoleteModule.UpgradeModule();
                    _listedModules[_listedModules.IndexOf(module)] = upgradedModule;
                    if (_indexMap.ContainsKey(index))
                    {
                        _indexMap[index] = upgradedModule.GetType();
                    }
                    IoC.Get<INewsPrinter>().EnqueueNews($"Upgraded {name} to {upgradedModule.Name}");
                    module = upgradedModule;
                }

                module.Enable(IoC, IO);
                IoC.Get<INewsPrinter>().EnqueueNews($"+{name} enabled.");
            }
            else
            {
                module.Disable();
                IoC.Get<INewsPrinter>().EnqueueNews($"+{name} disabled.");

                if (module is IObsoleteModule obsoleteModule && upgrade)
                {
                    var upgradedModule = obsoleteModule.UpgradeModule();
                    _listedModules[_listedModules.IndexOf(module)] = upgradedModule;
                    if (_indexMap.ContainsKey(index))
                    {
                        _indexMap[index] = upgradedModule.GetType();
                    }
                    IoC.Get<INewsPrinter>().EnqueueNews($"Upgraded {name} to {upgradedModule.Name}");
                }
            }
        }
    }
}