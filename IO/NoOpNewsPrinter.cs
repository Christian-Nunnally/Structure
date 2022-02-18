using Structure.IO.Output;

namespace Structure.IO
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
