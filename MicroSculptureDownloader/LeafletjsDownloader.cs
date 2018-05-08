using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.Primitives.Point;
using Size = SixLabors.Primitives.Size;

namespace MicroSculptureDownloader
{
    public class LeafletjsDownloader
    {
        // TODO: we need to limit the number of parallel downloads. 4 is hardcoded for now.
        private static readonly ParallelOptions ParallelOptions = new ParallelOptions{MaxDegreeOfParallelism = 4};
      
        /// <summary>
        /// Caller needs to dispose image!
        /// </summary>
        public static Image<Rgb24> Download(Func<TileCoordinates, string> urlGenerator, int tileSize, int totalSize)
        {
            var result = new Image<Rgb24>(totalSize, totalSize);
            var tileCount = totalSize / tileSize;

            Console.Write("Loading [");
            double done = 0.0, step = (tileCount * tileCount) / 100.0;
            var progressBarLock = new object();

            var rows = Enumerable.Range(start: 0, count: tileCount);
            var columns = Enumerable.Range(start: 0, count: tileCount);
            var tiles = rows.SelectMany(row => columns.Select(column => new TileCoordinates { Row = row, Column = column }));

            // Iterate over all tiles
            Parallel.ForEach(tiles, ParallelOptions, tile =>
            {
                WriteTile(tile, urlGenerator(tile), tileSize, result);
                lock (progressBarLock)
                {
                    done++;
                    while (done >= step)
                    {
                        done -= step;
                        Console.Write(".");
                    }
                }
            });

            Console.WriteLine("]");
            return result;
        }

        private static void WriteTile(TileCoordinates tileCoordinates, string url, int tileSize, Image<Rgb24> result)
        {
            try
            {
                // Download tile
                var data = new WebClient().DownloadData(url);

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

        public struct TileCoordinates
        {
            public int Row { get; set; }

            public int Column { get; set; }
        }
    }
}