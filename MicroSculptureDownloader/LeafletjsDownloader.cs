using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.Primitives.Point;

namespace MicroSculptureDownloader
{
    /// <summary>
    /// Downloads images or maps from any leaflet js powered website. https://leafletjs.com/
    /// </summary>
    public class LeafletjsDownloader
    {
        // TODO: we need to limit the number of parallel downloads. 4 is hardcoded for now.
        private static readonly ParallelOptions ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        /// <summary>
        /// Caller needs to dispose image!
        /// </summary>
        public static Image<Rgb24> Download(Func<TileCoordinates, string> urlGenerator, int tileSize, int totalSize, IProgressBar parentProgressBar = null)
        {
            var result = new Image<Rgb24>(totalSize, totalSize);
            var tileCount = totalSize / tileSize;

            var rows = Enumerable.Range(start: 0, count: tileCount);
            var columns = Enumerable.Range(start: 0, count: tileCount);
            var tiles = rows
                       .SelectMany(row => columns.Select(column => new TileCoordinates { Row = row, Column = column }))
                       .ToList();

            // Iterate over all tiles
            using (var progressBar = parentProgressBar?.Spawn(tiles.Count, "Downloading tiles"))
            {
                Parallel.ForEach(tiles, ParallelOptions, tile =>
                {
                    WriteTile(tile, urlGenerator(tile), tileSize, result);
                    progressBar?.Tick();
                });
            }

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

        /// <summary>
        /// Coordinates of a single leaflet tile.
        /// </summary>
        public struct TileCoordinates
        {
            /// <summary>
            /// Gets or sets the row index of a tile.
            /// </summary>
            public int Row { get; set; }

            /// <summary>
            /// Gets or sets the column index of a tile.
            /// </summary>
            public int Column { get; set; }
        }
    }
}
