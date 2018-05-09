namespace MicroSculptureDownloader.Extension
{
    /// <summary>
    /// Source to create a wallpaper from.
    /// </summary>
    public class WallpaperSource
    {
        /// <summary>
        /// Gets or sets the wallpaper file name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the source's file path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WallpaperSource"/> class.
        /// </summary>
        public static WallpaperSource Create(string name, string path)
        {
            return new WallpaperSource { Name = name, Path = path };
        }
    }
}
