using Structure.IO.Output;

namespace Structure.IO
{
    public interface INewsPrinter
    {
        bool PrintNews(IProgramOutput programOutput);

        void ClearNews();

        void EnqueueNews(string news);
    }
}