﻿using Structure.Modules;

namespace Structure
{
    public static class StartingModules
    {
        public static StructureModule[] CreateStartingModules() => new StructureModule[]
        {
            new TreeTask(),
            new Information(),
            new Diagnostics(),
            new Routiner(),
            new NewsArchive(),
            new TaskHistoryInformation(),
            new StartupStatistics(),
            new ModuleManager(),
        };
    }
}