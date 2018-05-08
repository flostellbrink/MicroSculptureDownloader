using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShellProgressBar;
using SixLabors.ImageSharp;

namespace MicroSculptureDownloader
{
    /// <summary>
    /// The program entry point.
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
        {
            // Define list of insects on microSculptures.net (by hand)
            var insects = new[]
            {
                "splendid-necked-dung-beetle", "orchid-bee-side", "tiger-beetle", "flying-saucer-trench-beetle",
                "ground-beetle-china", "jewel-longhorn-beetle", "orange-netted-winged-beetle", "marion-flightless-moth",
                "orchid-bee-top", "wasp-mimic-hoverfly", "tortoise-beetle", "treehopper", "branch-backed-treehopper",
                "blow-fly", "darkling-beetle", "ground-beetle", "tricolored-jewel-beetle", "burrowing-ground-beetle",
                "green-tiger-beetle", "common-reed-beetle", "lantern-bug", "mantis-fly",
                "amazonian-purple-warrior-scarab", "paris-peacock", "pleasing-fungus-beetle", "potter-wasp",
                "ruby-tailed-wasp", "shield-bug", "silver-longhorn-beetle", "stalk-eyed-fly",
                "white-short-nosed-weevil", "stag-beetle", "iridescent-bark-mantis", "dead-leaf-grasshopper",
            };

            const int subSteps = 2;
            using (var progressBar = new ProgressBar(subSteps, "Verifying insect list"))
            {
                // See if all of them exist
                using (var verificationProgressBar = progressBar.Spawn(insects.Length, "Verifying insects"))
                {
                    Parallel.ForEach(
                        insects,
                        insect =>
                        {
                            verificationProgressBar.Tick($"{insect}: {new MicroSculptureImage(insect).TileFolder}");
                        });
                }

                progressBar.Tick("Downloading all insects");

                // Create download folder
                const string downloadFolder = "download";
                Directory.CreateDirectory(downloadFolder);

                // Download images for all insects
                using (var downloadProgressBar = progressBar.Spawn(insects.Length, "Downloading all insects"))
                {
                    foreach (var insect in insects)
                    {
                        try
                        {
                            downloadProgressBar.Tick($"Downloading {insect}");
                            DownloadAllSizes(insect, downloadFolder, downloadProgressBar);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"Failed to download entire insect: {insect} - {exception.Message}");
                        }
                    }
                }

                progressBar.Tick("Download finished");
            }
        }

        /// <summary>
        /// Download all image sizes for a specific insect
        /// </summary>
        private static void DownloadAllSizes(string insect, string downloadFolder, IProgressBar parentProgressBar = null)
        {
            var downloader = new MicroSculptureImage(insect);
            var levels = downloader.GetLevels().ToList();

            var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
            using (var progressBar = parentProgressBar?.Spawn(levels.Count * 2, $"Downloading {insect}.", progressOptions))
            {
                foreach (var level in levels)
                {
                    try
                    {
                        progressBar?.Tick($"Downloading {insect} at level {level}");
                        using (var image = downloader.DownloadImage(level, null, progressBar))
                        using (var stream = new FileStream(
                            $"{downloadFolder}/{downloader.TileFolder}-{level}.png",
                            FileMode.Create))
                        {
                            progressBar?.Tick($"Saving {downloader.TileFolder}-{level}.png");
                            image.SaveAsPng(stream);
                        }

                        // Collect image, we don't want to look like we are hogging >>4 gigs
                        GC.Collect();
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
