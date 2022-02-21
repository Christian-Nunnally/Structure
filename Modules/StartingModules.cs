using Structure.IO.Persistence;
using Structure.Modules.Interface;
using Structure.Structure.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Structure.Modules
{
    public static class StartingModules
    {
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