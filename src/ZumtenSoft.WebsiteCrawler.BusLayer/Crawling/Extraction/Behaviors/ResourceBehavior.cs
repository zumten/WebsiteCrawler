using System.ComponentModel;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Crawling.Extraction.Behaviors
{
    /// <summary>
    /// Comportement du crawler quand il recontre une ressource
    /// </summary>
    public enum ResourceBehavior
    {
        Ignore,
        [Description("Download and stop")]
        DownloadAndStop,
        [Description("Follow redirection")]
        FollowRedirect,
        [Description("Follow HTML references")]
        FollowHtmlReferences,
        [Description("Follow all references")]
        FollowAllReferences,
    }
}