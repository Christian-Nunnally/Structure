namespace Structure.Modules.Interface
{
    internal interface IObsoleteModule
    {
        public IModule UpgradeModule();
    }
}