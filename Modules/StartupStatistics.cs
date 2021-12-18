using System.IO;
using System.Linq;

namespace Structure
{
    public class StartupStatistics : Module
    {
        protected override void OnEnable()
        {
            var allSaveFiles = Directory.GetFiles(FileIO.SavePath);
            var saveFileSizes = allSaveFiles.Select(x => new FileInfo(x).Length);
            var totalSize = saveFileSizes.Sum();
            totalSize /= 1024;
            var unit = "kb";
            if (totalSize > 1024)
            {
                totalSize /= 1024;
                unit = "mb";
            }
            IO.News($"Data = {totalSize} {unit}");
        }
    }
}