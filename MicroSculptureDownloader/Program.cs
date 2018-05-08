using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace MicroSculptureDownloader
{
    class Program
    {
        static void Main(string[] args)
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
                "white-short-nosed-weevil", "stag-beetle", "iridescent-bark-mantis", "dead-leaf-grasshopper"
            };

            // See if all of them exist
            Console.WriteLine("Verifying insects");

            Parallel.ForEach(insects,
                insect => Console.WriteLine($"{insect}: {new MircroSculptureImage(insect).TileFolder}"));

            Console.WriteLine("Looks good, start downloading");

            // Create download folder
            const string downloadFolder = "download";
            Directory.CreateDirectory(downloadFolder);
            
            // Download images for all insects
            var index = 0;
            foreach (var insect in insects)
            {
                try
                {
                    Console.WriteLine($"{++index}/{insects.Length} Locating {insect}");
                    DownloadAll(insect, downloadFolder);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Failed to download entire insect: {insect} - {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Download all image sizes for a specific insect
        /// </summary>
        private static void DownloadAll(string insect, string downloadFolder)
        {
            var downloader = new MircroSculptureImage(insect);
            Console.WriteLine($"Downloading {insect} from {downloader.TileFolder} with {downloader.ZoomLevels - 1} zoom levels.");
            foreach (var level in downloader.GetLevels())
            {
                try
                {
                    using (var image = downloader.DownloadImage(level))
                    using (var stream = new FileStream($"{downloadFolder}/{downloader.TileFolder}-{level}.png", FileMode.Create))
                    {
                        Console.Write($"\tSaving {downloader.TileFolder}-{level}.png ... ");
                        image.SaveAsPng(stream);
                        Console.WriteLine("done");
                    }
                    
                    // Collect image, we don't want to look like we are hogging >>4 gigs
                    GC.Collect();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Failed to download image for insect {insect} on level {level}- {exception.Message}");
                }
            }
        }
    }
}