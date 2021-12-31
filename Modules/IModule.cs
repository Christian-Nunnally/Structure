namespace Structure
{
    public interface IModule
    {
        string Name { get; }

        bool Enabled { get; }

        // TODO wrap these in ioc container;
        public void Enable(StructureIO io, Hotkey hotkey, StructureData data);

        void Disable();
    }
}