using Structure.Modules;

namespace Structure
{
    public static class StartingModules
    {
        public static StructureModule[] CreateStartingModules() => new StructureModule[]
        {
            new ModuleManager(),
            new TreeTask(),
            new Routiner(),
            new TaskHistoryInformation(),
        };
    }
}