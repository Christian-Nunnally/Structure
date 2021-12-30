using Structure.Modules;

namespace Structure
{
    public static class StartingModules
    {
        public static Module[] CreateStartingModules() => new Module[]
        {
            new TreeTask(),
            new Information(),
            new Diagnostics(),
            new Routiner(),
            new NewsArchive(),
            new CompletedTasks(),
            new StartupStatistics(),
            new ModuleManager(),
            new TaskHistoryInformation(),
        };
    }
}