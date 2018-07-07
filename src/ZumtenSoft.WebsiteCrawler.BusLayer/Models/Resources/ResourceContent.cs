using System.ComponentModel;
using System.Diagnostics;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    public enum ResourceContentKey
    {
        [Description("Page Title")]
        Title,
        Meta,
        Content,
        Search
    }

    [DebuggerDisplay(@"\{ResourceContent Key={Key}, Value={Value}\}")]
    public class ResourceContent
    {
        public ResourceContentKey Key { get; set; }
        public string SubKey { get; set; }
        public string Value { get; set; }
    }
}
