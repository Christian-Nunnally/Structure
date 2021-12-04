namespace Structure
{
    public abstract class Module : IModule
    {
        private bool _enabled;

        public string Name => GetType().Name;

        public bool Enabled => _enabled;

        public void Enable()
        {
            _enabled = true;
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