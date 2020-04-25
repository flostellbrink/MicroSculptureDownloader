using System;
using System.Collections.Generic;
using System.IO;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MicroSculptureDownloader.Extension
{
    /// <summary>
    /// Creates wallpapers from downloaded images.
    /// </summary>
    public class WallpaperCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WallpaperCreator"/> class.
        /// </summary>
        public WallpaperCreator(string wallpaperDirectory = "wallpaper")
        {
            WallpaperDirectory = Directory.CreateDirectory(wallpaperDirectory);
            Console.WriteLine($"Saving wallpapers in {WallpaperDirectory.FullName}");
        }

        /// <summary>
        /// Gets directory to save wallpapers in.
        /// </summary>
        public DirectoryInfo WallpaperDirectory { get; }

        /// <summary>
        /// Create a wallpaper from a downloaded image.
        /// </summary>
        public void CreateWallpapers(ICollection<WallpaperSource> sources, int width, int height, bool trim, int border)
        {
            ProgressBar progressBar = null;
            try
            {
                progressBar = new ProgressBar(sources.Count + 1, string.Empty);
            }
            catch
            {
                // Ignore progress bar failure.
            }

            foreach (var insect in sources)
            {
                progressBar?.Tick($"Creating wallpaper for {insect.Name}");
                try
                {
                    var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
                    using var wallpaperProgressBar = progressBar?.Spawn(trim ? 4 : 3, "Loading image", progressOptions);

                    var trimmed = trim ? "trimmed" : "full";
                    var wallpaperPath = $"{WallpaperDirectory}/{insect.Name}_{width}x{height}_{border}_{trimmed}.png";

                    using var inputFile = new FileStream(insect.Path, FileMode.Open);
                    using var inputImage = Image.Load<Rgb24>(inputFile);
                    using var outputImage = new Image<Rgb24>(width, height);
                    using var outputFile = new FileStream(wallpaperPath, FileMode.Create);

                    if (trim)
                    {
                        wallpaperProgressBar?.Tick("Trimming source image");
                        inputImage.Mutate(context => context.EntropyCrop(0.01f));
                    }

                    wallpaperProgressBar?.Tick("Resizing source image");
                    ResizeSource(inputImage, width, height, border);

                    wallpaperProgressBar?.Tick($"Writing wallpaper to {wallpaperPath}");
                    outputImage.Mutate(context => context.DrawImage(inputImage, 1.0f));
                    outputImage.SaveAsPng(outputFile);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in wallpaper {insect.Name} from {insect.Path} - {exception.Message}");
                }
            }
        }

        private void ResizeSource(Image<Rgb24> source, int width, int height, int border)
        {
            var factor = width / (double)source.Width;
            factor = source.Height * factor > height ? height / (double)source.Height : factor;

            source.Mutate(context => context
                                    .Resize((int)(source.Width * factor) - (2 * border), (int)(source.Height * factor) - (2 * border))
                                    .Pad(width, height));
        }
    }
}
