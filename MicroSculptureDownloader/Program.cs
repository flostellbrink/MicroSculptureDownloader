using System.IO;
using System.Linq;
using ShellProgressBar;

namespace MicroSculptureDownloader
{
    /// <summary>
    /// The program entry point.
    /// </summary>
    public class Program
    {
        private static readonly ImageCache Cache = new ImageCache();

        private static void Main(string[] args)
        {
            DownloadAll();
        }

        private static void DownloadAll()
        {
            // Create download folder
            const string downloadFolder = "download";
            

            using (var progressBar = new ProgressBar(2, "Collecting resolutions for all insects"))
            {
                // Get all zoom levels
                var allLevels = Cache.InsectList.Select(Cache.GetDownloader)
                                     .SelectMany(loader => loader.GetLevels())
                                     .Distinct()
                                     .OrderBy(level => level)
                                     .ToList();
                progressBar.Tick("Downloading images for all levels");

                // Download by zoom levels
                using (var downloadProgressBar = progressBar.Spawn(allLevels.Count, string.Empty))
                {
                    foreach (var level in allLevels)
                    {
                        downloadProgressBar.Tick($"Downloading resolution {level}");

                        var insects = Cache.InsectList
                                           .Where(insect => Cache.GetDownloader(insect).GetLevels().Contains(level))
                                           .ToList();

                        var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
                        using (var levelProgressBar = downloadProgressBar.Spawn(insects.Count * 2, string.Empty, progressOptions))
                        {
                            foreach (var insect in insects)
                            {
                                Cache.Get(insect, level, false, levelProgressBar);
                            }

                            levelProgressBar.Tick($"Downloaded resolution {level}");
                        }
                    }
                }
            }
        }
    }
}
