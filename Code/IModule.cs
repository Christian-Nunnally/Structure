namespace Structure
{
    public interface IModule
    {
        string Name { get; }

        void Enable();

        void Disable();
    }
}