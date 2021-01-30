namespace Structure
{
    public abstract class Module : IModule
    {
        public string Name => GetType().Name;

        public virtual void Disable()
        {
        }

        public virtual void Enable()
        {
        }
    }
}