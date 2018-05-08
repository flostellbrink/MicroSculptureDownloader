using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MicroSculptureDownloader
{
    public class MircroSculptureImage
    {
        public ReadOnlyCollection<ZoomModifier> ZoomModifiers { get; } =
            new ReadOnlyCollection<ZoomModifier>(new[] {new ZoomModifier(1, 256), new ZoomModifier(2, 512)});
        
        public int ZoomLevels { get; }
        
        public string TileFolder { get; }
        
        public int TotalSize { get; }
        
        public MircroSculptureImage(string imageName)
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
        }

        /// <summary>
        /// Caller needs to dispose image!
        /// </summary>
        public Image<Rgb24> DownloadImage(int? zoomLevel = null, ZoomModifier zoomModifier = null)
        {
            var modifier = zoomModifier ?? ZoomModifiers.Last();
            return DownloadImage(zoomLevel ?? ZoomLevels - modifier.Level, modifier);
        }

        /// <summary>
        /// Caller needs to dispose image!
        /// </summary>
        public Image<Rgb24> DownloadImage(int zoomLevel, ZoomModifier zoomModifier)
        {
            if(!ZoomModifiers.Contains(zoomModifier)) { throw new ArgumentOutOfRangeException(nameof(zoomModifier)); }
            if(zoomLevel + zoomModifier.Level > ZoomLevels) { throw new ArgumentOutOfRangeException(nameof(zoomLevel));}
            
            // Determine number of tiles for current zoom level & modifier
            var tileCount = ((TotalSize / zoomModifier.TileSize - 1)  >> (ZoomLevels - zoomLevel - zoomModifier.Level)) + 1;
            
            string UrlGen(LeafletjsDownloader.TileCoordinates tileCoordinates) =>
                $"http://microsculpture.net/assets/img/tiles/{TileFolder}/{zoomModifier}/{zoomLevel}/{tileCoordinates.Column}/{tileCoordinates.Row}.jpg";
            return LeafletjsDownloader.Download(UrlGen, zoomModifier.TileSize, tileCount * zoomModifier.TileSize);
        }

        public IEnumerable<int> GetLevels(ZoomModifier zoomModifier = null)
        {
            zoomModifier = zoomModifier ?? ZoomModifiers.Last();
            if(!ZoomModifiers.Contains(zoomModifier)) { throw new ArgumentOutOfRangeException(nameof(zoomModifier)); }
            return Enumerable.Range(0, ZoomLevels - zoomModifier.Level + 1);
        }

        public class ZoomModifier
        {
            public ZoomModifier(int level, int tileSize)
            {
                Level = level;
                TileSize = tileSize;
            }

            public int Level { get; }
            
            public int TileSize { get; }

            public override string ToString() => $"x{Level}";
        }
    }
}