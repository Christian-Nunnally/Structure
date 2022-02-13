using Structure.IO;
using Structure.Structure;

namespace Structure.Modules
{
    public abstract class StructureModule : IModule
    {
        protected StructureData Data { get; private set; }

        protected StructureIO IO { get; private set; }

        protected Hotkey Hotkey { get; private set; }

        public string Name => GetType().Name;

        public bool Enabled { get; private set; }

        public void Enable(StructureIO io, Hotkey hotkey, StructureData data)
        {
            Enabled = true;
            Data = data;
            IO = io;
            Hotkey = hotkey;
            OnEnable();
        }

        public void Disable()
        {
            Enabled = false;
            OnDisable();
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }
    }
}