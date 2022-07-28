using Structure.IO;
using Structure.Program;
using Structure.Program.Utilities;

namespace Structure.Modules.Interface
{
    public abstract class StructureModule : IModule
    {
        protected StructureIoC IoC { get; private set; }

        protected StructureData Data { get; private set; }

        protected StructureIO IO { get; private set; }

        protected Hotkey Hotkey { get; private set; }

        public string Name => GetType().Name;

        public bool Enabled { get; private set; }

        public void Enable(StructureIoC ioc, StructureIO io)
        {
            IoC = ioc;
            Enabled = true;
            Data = ioc?.Get<StructureData>();
            IO = io;
            Hotkey = ioc?.Get<Hotkey>();
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