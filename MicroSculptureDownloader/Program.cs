using System;
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
            Directory.CreateDirectory(downloadFolder);

            // Download images for all insects
            using (var downloadProgressBar = new ProgressBar(Cache.InsectList.Count, "Downloading all insects"))
            {
                foreach (var insect in Cache.InsectList)
                {
                    try
                    {
                        downloadProgressBar.Tick($"Downloading {insect}");
                        DownloadAllSizes(insect, downloadProgressBar);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Failed to download entire insect: {insect} - {exception.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Download all image sizes for a specific insect
        /// </summary>
        private static void DownloadAllSizes(string insect, IProgressBar parentProgressBar = null)
        {
            var levels = Cache.GetDownloader(insect).GetLevels().ToList();

            var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
            using (var progressBar = parentProgressBar?.Spawn(levels.Count * 2, $"Downloading {insect}.", progressOptions))
            {
                foreach (var level in levels)
                {
                    try
                    {
                        Cache.Get(insect, level, false, progressBar);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(
                            $"Failed to download image for insect {insect} on level {level}- {exception.Message}");
                    }
                }

                progressBar?.Tick($"Downloaded {insect}");
            }
        }
    }
}
