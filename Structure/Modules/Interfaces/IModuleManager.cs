namespace Structur.Modules.Interfaces
{
    internal interface IModuleManager : IModule
    {
        void RegisterModules(IModule[] modules);
    }
}