using System.Collections.Generic;
using System.IO;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

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
            Directory.CreateDirectory(wallpaperDirectory);
            WallpaperDirectory = wallpaperDirectory;
        }

        /// <summary>
        /// Gets directory to save wallpapers in.
        /// </summary>
        public string WallpaperDirectory { get; }

        /// <summary>
        /// Create a wallpaper from a downloaded image.
        /// </summary>
        public void CreateWallpapers(ICollection<WallpaperSource> sources, int width, int height, bool trim)
        {
            using (var progressBar = new ProgressBar(sources.Count + 1, string.Empty))
            {
                foreach (var insect in sources)
                {
                    progressBar.Tick($"Creating wallpaper for {insect.Name}");

                    var progressOptions = new ProgressBarOptions { CollapseWhenFinished = false };
                    using (var wallpaperProgressBar = progressBar.Spawn(3, "Loading image", progressOptions))
                    {
                        var trimmed = trim ? "trimmed" : "full";
                        var wallpaperPath = $"{WallpaperDirectory}/{insect.Name}_{width}x{height}_{trimmed}.png";

                        using (var inputFile = new FileStream(insect.Path, FileMode.Open))
                        using (var inputImage = Image.Load<Rgb24>(inputFile))
                        using (var outputImage = new Image<Rgb24>(width, height))
                        using (var outputFile = new FileStream(wallpaperPath, FileMode.Create))
                        {
                            // TODO trim image
                            wallpaperProgressBar.Tick("Resizing source image");
                            ResizeSource(inputImage, width, height);

                            wallpaperProgressBar.Tick($"Writing wallpaper to {wallpaperPath}");
                            var offset = (outputImage.Size() - inputImage.Size()) / 2;
                            outputImage.Mutate(context =>
                                context.DrawImage(GraphicsOptions.Default, inputImage, new Point(offset)));
                            outputImage.SaveAsPng(outputFile);
                        }
                    }
                }
            }
        }

        private void ResizeSource(Image<Rgb24> source, int width, int height)
        {
            // TODO add border
            var factor = width / (double)source.Width;
            factor = source.Height * factor > height ? height / (double)source.Height : factor;
            source.Mutate(context => context.Resize((int)(source.Width * factor), (int)(source.Height * factor)));
        }
    }
}
