namespace Structure.Modules.Interface
{
    internal interface IModuleManager : IModule
    {
        void RegisterModules(IModule[] modules);
    }
}