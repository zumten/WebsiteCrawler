namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    /// <summary>
    /// Type de référence vers une ressource.
    /// </summary>
    public enum ResourceReferenceTypes
    {
        /// <summary>
        /// Référence utilisée par le redirections 301 et 302
        /// </summary>
        Redirection,

        /// <summary>
        /// Correspond au tag HTML Anchor
        /// </summary>
        Anchor,

        /// <summary>
        /// Correspond au tag HTML Frame or IFrame
        /// </summary>
        Frame,

        /// <summary>
        /// Correspond au tag HTML IMG
        /// </summary>
        Image,

        /// <summary>
        /// Correspond au tag HTML SCRIPT
        /// </summary>
        Script,

        /// <summary>
        /// Correspond au tag HTML LINK. Aussi utilisé pour les
        /// liens du Sitemap.
        /// </summary>
        Link
    }

    public enum ReferenceSubType
    {
        None,

        // Link tag ref attribute
        Alternate,
        Archives,
        Author,
        Bookmark,
        Canonical,
        External,
        First,
        Help,
        Icon,
        Last,
        License,
        Next,
        NoFollow,
        NoReferrer,
        Pingback,
        Prefetch,
        Prev,
        Search,
        SideBar,
        StyleSheet,
        Tag,
        Up,

        // Types of redirection
        Redirect301 = 301,
        Redirect302 = 302,
        Redirect303 = 303,
        Redirect307 = 307,

        // Other
        ShortcutIcon,
        JavaScript,
        Sitemap,

        Frame,
        IFrame,
        Fallback
    }
}
