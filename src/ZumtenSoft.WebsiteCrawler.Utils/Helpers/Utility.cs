using System.IO;
using System.Linq;

namespace ZumtenSoft.WebsiteCrawler.Utils.Helpers
{
    public static class Utility
    {
        public static string PathCombine(params string[] parts)
        {
            return parts.Aggregate(Path.Combine);
        }
    }
}
