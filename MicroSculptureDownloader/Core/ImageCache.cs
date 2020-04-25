using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MicroSculptureDownloader.Extension;
using ShellProgressBar;
using SixLabors.ImageSharp;

namespace MicroSculptureDownloader.Core
{
    /// <summary>
    /// Cache for skipping image downloads.
    /// </summary>
    public class ImageCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCache"/> class.
        /// </summary>
        /// <param name="cacheDirectory">Directory to cache images in.</param>
        public ImageCache(string cacheDirectory = "download")
        {
            CacheDirectory = Directory.CreateDirectory(cacheDirectory);
            Console.WriteLine($"Caching images in {CacheDirectory.FullName}");

            var homePage = new WebClient().DownloadString("http://microsculpture.net/");
            var insectPattern = "<a href=\"(?<insect>[a-z\\-]+)\\.html\">";
            var insectMatches = Regex.Matches(homePage, insectPattern);
            InsectList = insectMatches.Select(match => match.Groups["insect"].Value).ToList();
        }

        /// <summary>
        /// Gets a list of all available insects.
        /// </summary>
        public IReadOnlyCollection<string> InsectList { get; }

        private ConcurrentDictionary<string, MicroSculptureImage> MicroSculptureImageCache { get; } =
            new ConcurrentDictionary<string, MicroSculptureImage>();

        private DirectoryInfo CacheDirectory { get; }

        /// <summary>
        /// Downloads the given list of insects.
        /// </summary>
        /// <returns>List of file paths.</returns>
        public IEnumerable<WallpaperSource> Download(IReadOnlyCollection<string> insectNames, int? requestedLevel, bool forceDownload)
        {
            ProgressBar progressBar = null;
            try
            {
                progressBar = new ProgressBar(2, "Collecting resolutions for all insects");
            }
            catch
            {
                // Ignore progress bar failure.
            }

            // Get all zoom levels
            var allLevels = requestedLevel.HasValue
                ? new List<int> { requestedLevel.Value }
                : insectNames.Select(GetDownloader)
                    .SelectMany(loader => loader.GetLevels())
                    .Distinct()
                    .OrderBy(level => level)
                    .ToList();
            progressBar?.Tick("Downloading images for these resolutions: " + string.Join(", ", allLevels));

            // Download by zoom levels
            using var downloadProgressBar = progressBar?.Spawn(allLevels.Count + 1, string.Empty);
            foreach (var level in allLevels)
            {
                downloadProgressBar?.Tick($"Downloading resolution {level}");

                var insects = insectNames
                    .Where(insect => GetDownloader(insect).GetLevels().Contains(level))
                    .ToList();

                var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
                using var levelProgressBar = downloadProgressBar?.Spawn(insects.Count * 2, string.Empty, progressOptions);
                foreach (var insect in insects)
                {
                    yield return new WallpaperSource
                    {
                        Name = insect,
                        Path = Get(insect, level, forceDownload, levelProgressBar),
                    };
                }

                levelProgressBar?.Tick($"Downloaded resolution {level}");
            }
        }

        /// <summary>
        /// Get the path to a specific image. Uses already downloaded first.
        /// </summary>
        public string Get(string insect, int? level = null, bool forceDownload = false, IProgressBar progressBar = null)
        {
            progressBar?.Tick($"Downloading {insect} at level {level}");

            var downloader = GetDownloader(insect);
            var path = $"{CacheDirectory}/{downloader.TileFolder}-{level}.png";
            if (File.Exists(path))
            {
                progressBar?.Tick($"Using existing from {path}");
                return path;
            }

            using (var image = downloader.DownloadImage(level, null, progressBar))
            {
                using var stream = new FileStream(path, FileMode.Create);
                progressBar?.Tick($"Saving {path}");
                image.SaveAsPng(stream);
            }

            // Collect image, we don't want to look like we are hogging >>4 gigs
            GC.Collect();
            return path;
        }

        /// <summary>
        /// Get a cached image downloader.
        /// </summary>
        public MicroSculptureImage GetDownloader(string insect)
        {
            if (!MicroSculptureImageCache.ContainsKey(insect))
            {
                MicroSculptureImageCache.TryAdd(insect, new MicroSculptureImage(insect));
            }

            return MicroSculptureImageCache[insect];
        }
    }
}
