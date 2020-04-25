using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MicroSculptureDownloader.Core
{
    /// <summary>
    /// Represents a specific MicroSculpture and allow downloading it at different resolutions.
    /// </summary>
    public class MicroSculptureImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicroSculptureImage"/> class.
        /// </summary>
        public MicroSculptureImage(string imageName)
        {
            var html = new WebClient().DownloadString($"http://microsculpture.net/{imageName}.html");
            const string pattern = "<div id=\"zoomTool\"[^>]*" +
                                   "data-map-bounds=\"(?<mapBounds>[^\"]*)\"[^>]*" +
                                   "data-tile-folder=\"(?<tileFolder>[^\"]*)\"[^>]*" +
                                   "data-max-zoom=\"(?<maxZoom>[^\"]*)\"[^>]*" +
                                   ">";
            var match = Regex.Match(html, pattern, RegexOptions.Singleline);

            TileFolder = match.Groups["tileFolder"].Value;
            TotalSize = int.Parse(match.Groups["mapBounds"].Value);
            ZoomLevels = int.Parse(match.Groups["maxZoom"].Value);

            MaxZoomLevels = ZoomLevels;
            while (!ValidZoomLevel(MaxZoomLevels - ZoomModifiers.Last().Level, ZoomModifiers.Last()))
            {
                MaxZoomLevels--;
            }
        }

        /// <summary>
        /// Gets the number of supported zoom levels.
        /// </summary>
        public int ZoomLevels { get; }

        /// <summary>
        /// Gets the number of supported zoom levels that do not lead to integer overflow.
        /// </summary>
        public int MaxZoomLevels { get; }

        /// <summary>
        /// Gets the directory where tiles are stored.
        /// </summary>
        public string TileFolder { get; }

        /// <summary>
        /// Gets the total image size.
        /// </summary>
        public int TotalSize { get; }

        private ReadOnlyCollection<ZoomModifier> ZoomModifiers { get; } =
            new ReadOnlyCollection<ZoomModifier>(new[] { new ZoomModifier(1, 256), new ZoomModifier(2, 512) });

        /// <summary>
        /// Caller needs to dispose image.
        /// </summary>
        public Image<Rgb24> DownloadImage(int? zoomLevel = null, ZoomModifier zoomModifier = null, IProgressBar parentProgressBar = null)
        {
            var modifier = zoomModifier ?? ZoomModifiers.Last();
            if (zoomLevel < 0 || zoomLevel > MaxZoomLevels - modifier.Level)
            {
                throw new ArgumentOutOfRangeException(nameof(zoomLevel));
            }

            return DownloadImage(zoomLevel ?? MaxZoomLevels - modifier.Level, modifier, parentProgressBar);
        }

        /// <summary>
        /// Get the list of all supported zoom levels.
        /// </summary>
        /// <param name="zoomModifier">The modifier to use for zoom levels.</param>
        public ICollection<int> GetLevels(ZoomModifier zoomModifier = null)
        {
            zoomModifier = zoomModifier ?? ZoomModifiers.Last();
            if (!ZoomModifiers.Contains(zoomModifier))
            {
                throw new ArgumentOutOfRangeException(nameof(zoomModifier));
            }

            return Enumerable.Range(start: 0, count: MaxZoomLevels - zoomModifier.Level + 1).ToList();
        }

        /// <summary>
        /// Get a resolution level, that is appropriate for a given wallpaper resolution.
        /// </summary>
        public int WallpaperLevel(int width, int height)
        {
            var maxDimension = Math.Max(width, height);
            var modifier = ZoomModifiers.Last();
            var result = GetLevels(modifier).Where(level => TileCount(level, modifier) * modifier.TileSize > maxDimension).ToList();
            return result.Any() ? result.First() : GetLevels().Last();
        }

        private Func<LeafletjsDownloader.TileCoordinates, string> UrlGen(int zoomLevel, ZoomModifier zoomModifier) => tileCoordinates =>
            $"http://microsculpture.net/assets/img/tiles/{TileFolder}/{zoomModifier}/{zoomLevel}/{tileCoordinates.Column}/{tileCoordinates.Row}.jpg";

        /// <summary>
        /// Caller needs to dispose image.
        /// </summary>
        private Image<Rgb24> DownloadImage(int zoomLevel, ZoomModifier zoomModifier, IProgressBar parentProgressBar = null)
        {
            if (!ZoomModifiers.Contains(zoomModifier))
            {
                throw new ArgumentOutOfRangeException(nameof(zoomModifier));
            }

            if (zoomLevel + zoomModifier.Level > ZoomLevels)
            {
                throw new ArgumentOutOfRangeException(nameof(zoomLevel));
            }

            var totalSize = TileCount(zoomLevel, zoomModifier) * zoomModifier.TileSize;
            return LeafletjsDownloader.Download(UrlGen(zoomLevel, zoomModifier), zoomModifier.TileSize, totalSize, parentProgressBar);
        }

        /// <summary>
        /// Determine number of tiles for zoom level and modifier.
        /// </summary>
        private int TileCount(int zoomLevel, ZoomModifier zoomModifier)
        {
            return (((TotalSize / zoomModifier.TileSize) - 1) >> (ZoomLevels - zoomLevel - zoomModifier.Level)) + 1;
        }

        /// <summary>
        /// We currently have a limitation on the maximum image size.
        /// ImageSharp and its array allocation is based on <see cref="int"/>. I.e. 32bit signed types.
        /// The entire image is stored in one array, if that gets bigger than this limit, everything breaks.
        /// This method allows filtering zoom levels before they break later on.
        /// </summary>
        private bool ValidZoomLevel(int zoomLevel, ZoomModifier zoomModifier)
        {
            var size = TileCount(zoomLevel, zoomModifier) * zoomModifier.TileSize;
            try
            {
                // 3 Bytes in Rgb24. Check for overflow
                var test = checked(size * size * 3);
                return test > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stores the zoom level modifier.
        /// Micro Sculputures can be viewed at different zoom levels, depending on client screen resolution.
        /// </summary>
        public class ZoomModifier
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ZoomModifier"/> class.
            /// </summary>
            /// <param name="level">The used zoom level.</param>
            /// <param name="tileSize">The size of tiles (along both dimensions).</param>
            public ZoomModifier(int level, int tileSize)
            {
                Level = level;
                TileSize = tileSize;
            }

            /// <summary>
            /// Gets the zoom level.
            /// </summary>
            public int Level { get; }

            /// <summary>
            /// Gets the tile size.
            /// </summary>
            public int TileSize { get; }

            /// <summary>
            /// Convert to a string that can be used in building an URL.
            /// </summary>
            public override string ToString() => $"x{Level}";
        }
    }
}
