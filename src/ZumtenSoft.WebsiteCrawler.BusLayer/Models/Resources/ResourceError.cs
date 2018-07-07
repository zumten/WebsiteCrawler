using System.Diagnostics;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources
{
    public enum ResourceErrorType
    {
        ParseHTML,
        ParseURL,
        ResolveURL
    }

    [DebuggerDisplay(@"\{ResourceError Description={Description}, Value={Value}, Line={Line}\}")]
    public class ResourceError
    {
        public ResourceErrorType Type { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }
        public int? Line { get; set; }
    }
}
