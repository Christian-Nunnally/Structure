namespace Structure
{
    public static class Modules
    {
        private static IModule[] _user;

        public static IModule[] UserModules => _user ?? (_user = new Module[]
        {
            new TreeTask(),
            new Information(),
            new CodeEditor(),
            new Routiner(),
            new Diagnostics(),
            new Weight(),
            new NewsArchive(),
            new CompletedTasks(),
            new StartupStatistics(),
            new ModuleManager(),
        });
    }
}