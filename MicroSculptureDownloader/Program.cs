using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using MicroSculptureDownloader.Core;
using MicroSculptureDownloader.Extension;
using ShellProgressBar;

namespace MicroSculptureDownloader
{
    /// <summary>
    /// The program entry point.
    /// </summary>
    [Command(Name = "MicroSculptureDownloader", Description = "Unofficial downloader for the brilliant insect photographs on http://microsculpture.net/")]
    [HelpOption("-?|--help")]
    public class Program
    {
        private static readonly ImageCache Cache = new ImageCache();

        private static readonly WallpaperCreator Creator = new WallpaperCreator();

        [Argument(order: 0, Name = "insect", Description = "Insect to download")]
        private string Insect { get; }

        [Option("-f|--force", Description = "Skip cache and force downloads.")]
        private bool ForceDownload { get; }

        [Option("-l|--level", Description = "Request specific image resolution.")]
        private int? Level { get; set; }

        [Option("-L|--list", Description = "List all insects.")]
        private bool ListInsects { get; }

        [Option("-r|--resolutions", Description = "List all resolutions.")]
        private bool ListResolutions { get; }

        [Option("-g|--generate", Description = "Whether to generate wallpapers. (Defaults to true)")]
        private bool WallpaperGenerate { get; } = true;

        [Option("-w|--width", Description = "Width of generated wallpapers. (Defaults to 3840)")]
        private int WallpaperWidth { get; } = 3840;

        [Option("-h|--height", Description = "Height of generated wallpapers. (Defaults to 2160)")]
        private int WallpaperHeight { get; } = 2160;

        [Option("-t|--trim", Description = "Whether to trim wallpapers, i.e. remove borders. (Defaults to true)")]
        private bool WallpaperTrim { get; } = true;

        [Option("-b|--border", Description = "Uniform border around wallpaper. (Defaults to 100)")]
        private int WallpaperBorder { get; } = 100;

        private static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app)
        {
            // Check parameters
            if (WallpaperWidth <= 0 || WallpaperHeight <= 0)
            {
                app.Error.WriteLine("Wallpaper width and height need to be positive.");
                app.ShowHint();
                return;
            }

            if (WallpaperBorder <= 0 || WallpaperBorder >= WallpaperWidth || WallpaperBorder >= WallpaperHeight)
            {
                app.Error.WriteLine("Wallpaper border must be positive and cannot be larger than the width or height.");
                app.ShowHint();
                return;
            }

            // List all insects
            if (ListInsects)
            {
                foreach (var insect in Cache.InsectList)
                {
                    app.Out.WriteLine(insect);
                }

                return;
            }

            // All other commands require a list of insects to work on
            if (!TryGetInsects(app, out var insectNames) || !insectNames.Any())
            {
                return;
            }

            // List all resolutions for each selected insect
            if (ListResolutions)
            {
                var maxWidth = insectNames.Select(name => name.Length).Max();
                foreach (var insectName in insectNames)
                {
                    app.Out.WriteLine($"{insectName.PadRight(maxWidth)} {string.Join(", ", Cache.GetDownloader(insectName).GetLevels())}");
                }

                return;
            }

            /*
             * Find an appropriate quality level for wallpaper generation
             * This is just a heuristic, because:
             * - When trimming, the resolution might drop under the given size.
             * - Different images might have different quality definitions.
             */
            if (!Level.HasValue && WallpaperGenerate)
            {
                var dummyDownloader = Cache.GetDownloader(insectNames.First());
                Level = dummyDownloader.WallpaperLevel(WallpaperWidth, WallpaperHeight);
            }

            // Download all selected insects
            var insects = Download(insectNames).ToList();

            // Create wallpapers
            if (WallpaperGenerate)
            {
                var sources = insectNames.Zip(insects, WallpaperSource.Create).ToList();
                Creator.CreateWallpapers(sources, WallpaperWidth, WallpaperHeight, WallpaperTrim, WallpaperBorder);
            }
        }

        private bool TryGetInsects(CommandLineApplication app, out IReadOnlyCollection<string> insectNames)
        {
            if (string.IsNullOrWhiteSpace(Insect))
            {
                insectNames = Cache.InsectList;
                return true;
            }

            if (!Cache.InsectList.Contains(Insect.ToLower().Trim()))
            {
                app.Error.WriteLine($"Could not find {Insect}. Use \"--list\" to show valid insects.");
                app.ShowHint();
                insectNames = null;
                return false;
            }

            if (Level.HasValue && !Cache.GetDownloader(Insect.ToLower().Trim()).GetLevels().Contains(Level.Value))
            {
                app.Error.WriteLine($"Could not find resolution {Level} on insect {Insect}. Use \"--resolutions\" to show valid resolutions for an insect.");
                app.ShowHint();
                insectNames = null;
                return false;
            }

            insectNames = new[] { Insect.ToLower().Trim() };
            return true;
        }

        private IEnumerable<string> Download(IReadOnlyCollection<string> insectNames)
        {
            using var progressBar = new ProgressBar(2, "Collecting resolutions for all insects");

            // Get all zoom levels
            var allLevels = Level.HasValue
                ? new List<int> { Level.Value }
                : insectNames.Select(Cache.GetDownloader)
                    .SelectMany(loader => loader.GetLevels())
                    .Distinct()
                    .OrderBy(level => level)
                    .ToList();
            progressBar.Tick("Downloading images for these resolutions: " + string.Join(", ", allLevels));

            // Download by zoom levels
            using var downloadProgressBar = progressBar.Spawn(allLevels.Count + 1, string.Empty);
            foreach (var level in allLevels)
            {
                downloadProgressBar.Tick($"Downloading resolution {level}");

                var insects = insectNames
                    .Where(insect => Cache.GetDownloader(insect).GetLevels().Contains(level))
                    .ToList();

                var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
                using var levelProgressBar = downloadProgressBar.Spawn(insects.Count * 2, string.Empty, progressOptions);
                foreach (var insect in insects)
                {
                    yield return Cache.Get(insect, level, ForceDownload, levelProgressBar);
                }

                levelProgressBar.Tick($"Downloaded resolution {level}");
            }
        }
    }
}
