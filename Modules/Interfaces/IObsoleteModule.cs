namespace Structur.Modules.Interfaces
{
    internal interface IObsoleteModule
    {
        public IModule UpgradeModule();
    }
}