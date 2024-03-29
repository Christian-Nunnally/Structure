﻿using Structur.IO.Persistence;
using Structur.Modules.Interfaces;
using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Structur.Modules
{
    public static class StartingModules
    {
        [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetLoadableTypes()")]
        public static IModule[] Create()
        {
            var allModuleTypes = GetLoadableModuleTypes();
            var subModuleTypes = allModuleTypes.Where(typeof(ISubModule).IsAssignableFrom);
            var moduleTypes = allModuleTypes.Except(subModuleTypes);
            var startingModules = DetermineModulesThatUpgradeIntoAllOtherModules(moduleTypes);
            var startingModuleInstances = startingModules.Select(Activator.CreateInstance);
            return startingModuleInstances.OfType<IModule>().ToArray();
        }

        private static List<Type> DetermineModulesThatUpgradeIntoAllOtherModules(IEnumerable<Type> moduleTypes)
        {
            var obsolete = moduleTypes.Where(typeof(IObsoleteModule).IsAssignableFrom).ToList();
            var nonObsolete = moduleTypes.Except(obsolete).ToList();
            if (ShouldUseObsoleteModules())
            {
                bool keepLooping = true;
                while (keepLooping)
                {
                    keepLooping = false;
                    for (int i = obsolete.Count - 1; i >= 0; i--)
                    {
                        var obsoleteType = obsolete[i];
                        var typeOfUpgrade = GetTypeOfUpgrade(obsoleteType);
                        if (nonObsolete.Contains(typeOfUpgrade)) nonObsolete[nonObsolete.IndexOf(typeOfUpgrade)] = obsoleteType;
                        else if (obsolete.Contains(typeOfUpgrade)) keepLooping = true;
                        else obsolete.Remove(obsoleteType);
                    }
                }
            }
            return nonObsolete;
        }

        private static bool ShouldUseObsoleteModules()
        {
            return new PersistedList<bool>("use-obsolete-modules").FirstOrDefault();
        }

        private static Type GetTypeOfUpgrade(Type obsoleteModuleType)
        {
            var obsoleteModule = (IObsoleteModule)Activator.CreateInstance(obsoleteModuleType);
            var newModule = obsoleteModule.UpgradeModule();
            return newModule.GetType();
        }

        [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetLoadableTypes()")]
        private static IEnumerable<Type> GetLoadableModuleTypes()
        {
            var moduleType = typeof(IModule);
            var assembly = moduleType.Assembly;
            return assembly.GetLoadableTypes()
                .Where(moduleType.IsAssignableFrom)
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .ToList();
        }
    }
}