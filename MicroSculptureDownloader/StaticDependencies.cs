using System.Net;

namespace MicroSculptureDownloader
{
    // Should probably be replaced with IOT
    public static class StaticDependencies
    {
        public static WebClient WebClient { get; } = new WebClient();
    }
}