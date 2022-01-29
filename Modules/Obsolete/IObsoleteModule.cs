namespace Structure.Code.Modules
{
    internal interface IObsoleteModule
    {
        public IModule UpgradeModule();
    }
}