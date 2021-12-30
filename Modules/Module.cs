namespace Structure
{
    public abstract class Module : IModule
    {
        private bool _enabled;
        protected CommonData CurrentData;
        protected StructureIO IO;
        protected Hotkey Hotkey;

        public string Name => GetType().Name;

        public bool Enabled => _enabled;

        public void Enable(StructureIO io, Hotkey hotkey, CommonData data)
        {
            _enabled = true;
            CurrentData = data;
            IO = io;
            Hotkey = hotkey;
            OnEnable();
        }

        public void Disable()
        {
            _enabled = false;
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