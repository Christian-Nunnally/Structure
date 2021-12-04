namespace Structure
{
    public interface IModule
    {
        string Name { get; }

        bool Enabled { get; }

        void Enable();

        void Disable();
    }
}