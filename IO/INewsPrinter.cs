using Structure.IO.Output;

namespace Structure.IO
{
    public interface INewsPrinter : IBackgroundProcess
    {
        bool PrintNews(IProgramOutput programOutput);

        void EnqueueNews(string news);

        void Disable();

        void Enable();
    }
}