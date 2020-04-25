using System.Linq;
using MicroSculptureDownloader.Core;
using MicroSculptureDownloader.Extension;
using Xunit;

namespace MicroSculptureDownloader.Test
{
    public class Integration
    {
        [Fact]
        public void GetInsects()
        {
            var cache = new ImageCache();
            Assert.NotEmpty(cache.InsectList);
            Assert.NotEmpty(cache.InsectList.First());
        }

        [Fact]
        public void DownloadWallpaper()
        {
            var cache = new ImageCache();
            var creator = new WallpaperCreator();

            var insectNames = cache.InsectList.Take(2).ToList();
            var dummyDownloader = cache.GetDownloader(insectNames.First());
            var level = dummyDownloader.WallpaperLevel(1920, 1080);
            var insects = cache.Download(insectNames, level, true).ToList();
            creator.CreateWallpapers(insects, 1920, 1080, true, 50);
        }
    }
}

