using System;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.Primitives.Point;
using Size = SixLabors.Primitives.Size;

namespace MicroSculptureDownloader
{
    public class LeafletjsDownloader
    {
        /// <summary>
        /// Caller needs to dispose image!
        /// </summary>
        public static Image<Rgb24> Download(Func<int, int, string> urlGenerator, int tileSize, int totalSize)
        {
            var result = new Image<Rgb24>(totalSize, totalSize);
            var tileCount = totalSize / tileSize;

            Console.Write("Loading [");
            double done = 0.0, step = (tileCount * tileCount) / 100.0;

            // Iterate over all tiles
            foreach (var row in Enumerable.Range(0, tileCount).AsParallel())
            foreach (var col in Enumerable.Range(0, tileCount).AsParallel())
            {
                WriteTile(row, col, urlGenerator(row, col), tileSize, result);
                lock (result)
                {
                    done++;
                    while (done >= step)
                    {
                        done -= step;
                        Console.Write(".");
                    }
                }
            }

            Console.WriteLine("]");
            return result;
        }

        private static void WriteTile(int row, int col, string url, int tileSize, Image<Rgb24> result)
        {
            try
            {
                // Download tile
                // Parse image and dispose of it later
                using (var tile = Image.Load<Rgb24>(data))
                {
                    // Add tile to result
                    var location = new Point(tileCoordinates.Column * tileSize, tileCoordinates.Row * tileSize);
                    lock (result)
                    {
                        result.Mutate(r => r.DrawImage(GraphicsOptions.Default, tile, location));
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in image {url} - {exception.Message}");
            }
        }
    }
}