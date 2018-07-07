namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    /// <summary>
    /// Type de contenu d'une ressource. On se fie sur le header Content-Type et sur l'URL.
    /// </summary>
    public enum ResourceContentType
    {
        /// <summary>
        /// Pas moyen de trouver le type de contenu. (redirection, ignored, timeout, error)
        /// </summary>
        Unknown,
        Robots,
        SiteMap,
        Html,
        Css,
        JavaScript,
        Redirect,
        Image,
        Other
    }
}