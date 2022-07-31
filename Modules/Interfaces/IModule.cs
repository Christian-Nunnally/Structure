using Structur.IO;
using Structur.Program;

namespace Structur.Modules.Interfaces
{
    public interface IModule
    {
        string Name { get; }

        bool Enabled { get; }

        public void Enable(StructureIoC ioc, StructureIO io);

        void Disable();
    }
}