namespace Structure
{
    public static class Modules
    {
        private static IModule[] _user;

        private static IModule[] _system;

        public static IModule[] UserModules => _user ?? (_user = new Module[]
        {
            new TreeTask(),
            new Information(),
            new CharacterBonusUpgrader(),
            new Prestieger(),
            new CodeEditor(),
            new Routiner(),
            new Diagnostics(),
            new Leveler(),
            new Weight(),
        });

        public static IModule[] SystemModules => _system ?? (_system = new Module[]
        {
            new Toxin(),
            new CharacterBonusModule(),
            new ModuleManager(),
            new Backup(),
            new StartupStatistics(),
        });
    }
}