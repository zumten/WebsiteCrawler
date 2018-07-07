using System;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class NumberHelper
    {
        public static int? TryParseInt32(string input)
        {
            int value;
            if (Int32.TryParse(input, out value))
                return value;
            return null;
        }

    }
}
