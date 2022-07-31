using Structur.IO.Output;

namespace Structur.IO
{
    public class NoOpNewsPrinter : INewsPrinter
    {
        public bool PrintNews(IProgramOutput programOutput) => false;

        public void EnqueueNews(string news) { }

        public bool DoProcess(StructureIO io) => false;

        public void Disable() { }

        public void Enable() { }
    }
}
